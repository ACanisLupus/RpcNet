// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using PortMapper;

// Public for tests
public class RpcTcpServer : IDisposable
{
    private readonly Dictionary<Socket, RpcTcpConnection> _connections = new();
    private readonly IPAddress _ipAddress;
    private readonly ILogger? _logger;
    private readonly int _portMapperPort;
    private readonly int _program;
    private readonly Action<ReceivedRpcCall> _receivedCallDispatcher;
    private readonly Socket _server;
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
        ServerSettings? serverSettings = default)
    {
        serverSettings ??= new ServerSettings();

        _serverSettings = serverSettings;
        _program = program;
        _versions = versions;
        _receivedCallDispatcher = receivedCallDispatcher;
        _logger = serverSettings.Logger;
        _ipAddress = ipAddress;
        _port = port;
        _portMapperPort = serverSettings.PortMapperPort;
        _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public int Start()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(RpcUdpServer));
        }

        if (_acceptingThread != null)
        {
            return _port;
        }

        try
        {
            _server.Bind(new IPEndPoint(_ipAddress, _port));
            _server.Listen(int.MaxValue);
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not start TCP listener. Socket error: {e.SocketErrorCode}.");
        }

        if (_port == 0)
        {
            if (_server.LocalEndPoint is not IPEndPoint localEndPoint)
            {
                throw new InvalidOperationException("Could not find local endpoint for server socket.");
            }

            _port = localEndPoint.Port;

            if (_program != PortMapperConstants.PortMapperProgram)
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
                        PortMapperUtilities.UnsetAndSetPort(
                            ProtocolKind.Tcp,
                            _portMapperPort,
                            _port,
                            _program,
                            version,
                            clientSettings);
                    }
                }
            }
        }

        _logger?.Info($"{Utilities.ConvertToString(Protocol.Tcp)} Server listening on {_server.LocalEndPoint}...");

        _acceptingThread = new Thread(Accepting)
        {
            IsBackground = true,
            Name = $"RpcNet TCP Server Thread for Port {_port}"
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
            _server.Shutdown(SocketShutdown.Both);
        }
        catch
        {
            // Ignored
        }

        _server.Dispose();

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
                    foreach (Socket socket in _connections.Keys)
                    {
                        sockets.Add(socket);
                    }
                }

                sockets.Add(_server);

                Socket.Select(sockets, null, null, 1000000);

                lock (_connections)
                {
                    for (int i = sockets.Count - 1; i >= 0; i--)
                    {
                        if (sockets[i] == _server)
                        {
                            Socket tcpClient = _server.Accept();
                            var connection = new RpcTcpConnection(
                                tcpClient,
                                _program,
                                _versions,
                                _receivedCallDispatcher,
                                _logger);

                            _connections.Add(tcpClient, connection);
                        }
                        else
                        {
                            RpcTcpConnection connection = _connections[sockets[i]];
                            if (!connection.Handle())
                            {
                                connection.Dispose();
                                _connections.Remove(sockets[i]);
                            }
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                _logger?.Error($"Could not accept TCP client. Socket error: {e.SocketErrorCode}");
            }
            catch (Exception e)
            {
                _logger?.Error($"The following error occurred while accepting TCP clients: {e}");
            }
        }
    }
}
