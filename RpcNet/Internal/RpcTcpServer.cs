// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using RpcNet.PortMapper;

// Public for tests
public sealed class RpcTcpServer : IDisposable
{
    private readonly Dictionary<Socket, RpcTcpConnection> _connections = new();
    private readonly IPAddress _ipAddress;
    private readonly ILogger? _logger;
    private readonly int _portMapperPort;
    private readonly int _program;
    private readonly Action<ReceivedRpcCall> _receivedCallDispatcher;
    private readonly Socket _socket;
    private readonly int[] _versions;
    private readonly ServerSettings _serverSettings;

    private Thread? _acceptingThread;
    private bool _isDisposed;
    private int _port;
    private volatile bool _stopAccepting;

    public RpcTcpServer(
        IPAddress ipAddress,
        int port,
        int program,
        int[] versions,
        Action<ReceivedRpcCall> receivedCallDispatcher,
        ServerSettings serverSettings)
    {
        _serverSettings = serverSettings;
        _program = program;
        _versions = versions;
        _receivedCallDispatcher = receivedCallDispatcher;
        _logger = serverSettings.Logger;
        _ipAddress = ipAddress;
        _port = port;
        _portMapperPort = serverSettings.PortMapperPort;
        _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            try
            {
                _socket.DualMode = true;
            }
            catch (SocketException e)
            {
                _logger?.Error($"Could not enable dual mode. Socket error code: {e.SocketErrorCode}. Only IPv6 is available.");
            }
        }
    }

    public int Start()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(RpcTcpServer));
        }

        if (_acceptingThread is not null)
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

        if ((_program != PortMapperConstants.PortMapperProgram) && (_portMapperPort != 0))
        {
            lock (_connections)
            {
                var clientSettings = new ClientSettings
                {
                    Logger = _serverSettings.Logger,
                    ReceiveTimeout = _serverSettings.ReceiveTimeout,
                    SendTimeout = _serverSettings.SendTimeout
                };
                foreach (int version in _versions)
                {
                    PortMapperUtilities.UnsetAndSetPort(_ipAddress.AddressFamily, ProtocolKind.Tcp, _portMapperPort, _port, _program, version, clientSettings);
                }
            }
        }

        _logger?.Info($"TCP Server listening on {localEndPoint}...");

        _acceptingThread = new Thread(Accepting)
        {
            IsBackground = true,
            Name = $"RpcNet TCP Server Thread for {localEndPoint}"
        };
        _acceptingThread.Start();
        return _port;
    }

    public void Dispose()
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

        lock (_connections)
        {
            foreach (RpcTcpConnection connection in _connections.Values)
            {
                connection.Dispose();
            }

            _connections.Clear();
        }

        Interlocked.Exchange(ref _acceptingThread, null)?.Join();
        _isDisposed = true;
    }

    private void Accepting()
    {
        var sockets = new List<Socket>();
        while (!_stopAccepting)
        {
            try
            {
                sockets.Clear();
                lock (_connections)
                {
                    // + 1 for the server
                    sockets.Capacity = _connections.Count + 1;
                    foreach (Socket socket in _connections.Keys)
                    {
                        sockets.Add(socket);
                    }
                }

                sockets.Add(_socket);

                Socket.Select(sockets, null, null, 1000000);

                lock (_connections)
                {
                    for (int i = sockets.Count - 1; i >= 0; i--)
                    {
                        if (sockets[i] == _socket)
                        {
                            Socket acceptedSocket = _socket.Accept();
                            var connection = new RpcTcpConnection(acceptedSocket, _program, _versions, _receivedCallDispatcher, _logger);

                            _connections.Add(acceptedSocket, connection);
                        }
                        else
                        {
                            RpcTcpConnection connection = _connections[sockets[i]];
                            if (!connection.Handle())
                            {
                                connection.Dispose();
                                _ = _connections.Remove(sockets[i]);
                            }
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                if (!_stopAccepting)
                {
                    _logger?.Error($"Could not accept TCP client. Socket error code: {e.SocketErrorCode}");
                }
            }
            catch (Exception e)
            {
                if (!_stopAccepting)
                {
                    _logger?.Error($"The following error occurred while accepting TCP clients: {e}");
                }
            }
        }
    }
}
