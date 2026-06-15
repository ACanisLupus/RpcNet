// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using RpcNet.PortMapper;

internal sealed class RpcTcpServer : IAsyncDisposable
{
    private readonly Dictionary<RpcTcpConnection, Task> _connections = [];
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

        Task[] connectionTasks;
        await _lockConnections.WaitAsync();
        try
        {
            foreach (RpcTcpConnection connection in _connections.Keys)
            {
                connection.Dispose();
            }

            connectionTasks = [.. _connections.Values];
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

        foreach (Task connectionTask in connectionTasks)
        {
            try
            {
                await connectionTask.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _serverSettings.Logger?.Error($"The following error occurred while waiting for a connection task to finish: {e}");
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
        while (!_stopAccepting)
        {
            try
            {
                Socket acceptedSocket = await _socket.AcceptAsync(cancellationToken).ConfigureAwait(false);
                acceptedSocket.NoDelay = true;
                RpcTcpConnection connection = new(acceptedSocket, _program, _versions, _receivedCallDispatcher, _serverSettings.Logger);

                await _lockConnections.WaitAsync(cancellationToken);
                try
                {
                    if (_stopAccepting)
                    {
                        connection.Dispose();
                        break;
                    }

                    // Handle each client on its own task so that clients are served in parallel.
                    _connections.Add(connection, Task.Run(() => HandleConnectionAsync(connection, cancellationToken), cancellationToken));
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

    private async Task HandleConnectionAsync(RpcTcpConnection connection, CancellationToken cancellationToken)
    {
        try
        {
            while (!_stopAccepting && await connection.HandleAsync(cancellationToken).ConfigureAwait(false))
            {
            }
        }
        catch (Exception e)
        {
            if (!_stopAccepting)
            {
                _serverSettings.Logger?.Error($"The following error occurred while handling a TCP client: {e}");
            }
        }
        finally
        {
            await _lockConnections.WaitAsync(cancellationToken);
            try
            {
                _ = _connections.Remove(connection);
            }
            finally
            {
                _lockConnections.Release();
            }

            connection.Dispose();
        }
    }
}
