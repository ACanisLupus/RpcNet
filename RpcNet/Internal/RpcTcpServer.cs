// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using RpcNet.PortMapper;

internal sealed class RpcTcpServer : IAsyncDisposable
{
    private readonly Dictionary<Socket, RpcTcpConnection> _connections = [];
    private readonly IPAddress _ipAddress;
    private readonly SemaphoreSlim _lockConnections = new(1, 1);
    private readonly int _program;
    private readonly Func<ReceivedRpcCall, CancellationToken, ValueTask> _receivedCallDispatcher;
    private readonly ServerSettings _serverSettings;
    private readonly Socket _socket;
    private readonly int[] _versions;

    private Task? _acceptingTask;
    private bool _isDisposed;
    private int _port;
    private volatile bool _stopAccepting;

    public RpcTcpServer(
        IPAddress ipAddress,
        int port,
        int program,
        int[] versions,
        Func<ReceivedRpcCall, CancellationToken, ValueTask> receivedCallDispatcher,
        ServerSettings serverSettings)
    {
        _serverSettings = serverSettings;
        _program = program;
        _versions = versions;
        _receivedCallDispatcher = receivedCallDispatcher;
        _ipAddress = ipAddress;
        _port = port;
        _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            try
            {
                _socket.DualMode = true;
            }
            catch (SocketException e)
            {
                serverSettings.Logger?.Error($"Could not enable dual mode. Socket error code: {e.SocketErrorCode}. Only IPv6 is available.");
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _stopAccepting = true;
        try
        {
            // Necessary for Linux. Dispose doesn't abort synchronous calls
            _socket.Shutdown(SocketShutdown.Both);
        }
        catch
        {
            // Ignored
        }

        _socket.Dispose();

        await _lockConnections.WaitAsync();
        try
        {
            foreach (RpcTcpConnection connection in _connections.Values)
            {
                connection.Dispose();
            }

            _connections.Clear();
        }
        finally
        {
            _lockConnections.Release();
        }

        if (_acceptingTask is not null)
        {
            try
            {
                await _acceptingTask.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _serverSettings.Logger?.Error($"The following error occurred while waiting for the accepting task to finish: {e}");
            }
        }

        _isDisposed = true;
    }

    public async Task<int> StartAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, nameof(RpcTcpServer));

        if (_acceptingTask is not null)
        {
            return _port;
        }

        try
        {
            _socket.Bind(new IPEndPoint(_ipAddress, _port));
            _socket.Listen(int.MaxValue);
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not start TCP listener. Socket error code: {e.SocketErrorCode}.");
        }

        if (_socket.LocalEndPoint is not IPEndPoint localEndPoint)
        {
            throw new InvalidOperationException("Could not find local endpoint for server socket.");
        }

        if (_port == 0)
        {
            _port = localEndPoint.Port;
        }

        if ((_program != PortMapperConstants.PortMapperProgram) && (_serverSettings.PortMapperPort != 0))
        {
            await _lockConnections.WaitAsync(cancellationToken);
            try
            {
                ClientSettings clientSettings = new()
                {
                    Logger = _serverSettings.Logger,
                    ReceiveTimeout = _serverSettings.ReceiveTimeout,
                    SendTimeout = _serverSettings.SendTimeout
                };
                foreach (int version in _versions)
                {
                    await PortMapperUtilities.UnsetAndSetPortAsync(
                        _ipAddress.AddressFamily,
                        ProtocolKind.Tcp,
                        _serverSettings.PortMapperPort,
                        _port,
                        _program,
                        version,
                        clientSettings,
                        cancellationToken);
                }
            }
            finally
            {
                _lockConnections.Release();
            }
        }

        _serverSettings.Logger?.Info($"TCP Server listening on {localEndPoint}...");

        _acceptingTask = Task.Run(() => AcceptingAsync(cancellationToken), cancellationToken);
        return _port;
    }

    private async Task AcceptingAsync(CancellationToken cancellationToken)
    {
        List<Socket> sockets = [];
        while (!_stopAccepting)
        {
            try
            {
                sockets.Clear();
                await _lockConnections.WaitAsync(cancellationToken);
                try
                {
                    // + 1 for the server
                    sockets.Capacity = _connections.Count + 1;
                    sockets.AddRange(_connections.Keys);
                }
                finally
                {
                    _lockConnections.Release();
                }

                sockets.Add(_socket);

                Socket.Select(sockets, null, null, 1000000);

                await _lockConnections.WaitAsync(cancellationToken);
                try
                {
                    for (int i = sockets.Count - 1; i >= 0; i--)
                    {
                        if (sockets[i] == _socket)
                        {
                            Socket acceptedSocket = await _socket.AcceptAsync(cancellationToken).ConfigureAwait(false);
                            RpcTcpConnection connection = new(acceptedSocket, _program, _versions, _receivedCallDispatcher, _serverSettings.Logger);

                            _connections.Add(acceptedSocket, connection);
                        }
                        else
                        {
                            RpcTcpConnection connection = _connections[sockets[i]];
                            if (!await connection.HandleAsync(cancellationToken).ConfigureAwait(false))
                            {
                                connection.Dispose();
                                _ = _connections.Remove(sockets[i]);
                            }
                        }
                    }
                }
                finally
                {
                    _lockConnections.Release();
                }
            }
            catch (SocketException e)
            {
                if (!_stopAccepting)
                {
                    _serverSettings.Logger?.Error($"Could not accept TCP client. Socket error code: {e.SocketErrorCode}");
                }
            }
            catch (Exception e)
            {
                if (!_stopAccepting)
                {
                    _serverSettings.Logger?.Error($"The following error occurred while accepting TCP clients: {e}");
                }
            }
        }
    }
}
