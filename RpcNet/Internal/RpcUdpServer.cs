// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using RpcNet.PortMapper;

// Public for tests
public sealed class RpcUdpServer : IDisposable
{
    private readonly ILogger? _logger;
    private readonly int _port;
    private readonly UdpReader _reader;
    private readonly ReceivedRpcCall _receivedCall;
    private readonly Socket _server;
    private readonly UdpWriter _writer;

    private bool _isDisposed;
    private Thread? _receivingThread;
    private volatile bool _stopReceiving;

    public RpcUdpServer(
        IPAddress ipAddress,
        int port,
        int program,
        int[] versions,
        Action<ReceivedRpcCall> receivedCallDispatcher,
        ServerSettings? serverSettings = default)
    {
        serverSettings ??= new ServerSettings();

        _port = port;
        _server = new Socket(ipAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            try
            {
                _server.DualMode = true;
            }
            catch (SocketException e)
            {
                _logger?.Error($"Could not enable dual mode. Socket error code: {e.SocketErrorCode}. Only IPv6 is available.");
            }
        }

        _server.Bind(new IPEndPoint(ipAddress, _port));

        _reader = new UdpReader(_server);
        _writer = new UdpWriter(_server);

        _receivedCall = new ReceivedRpcCall(program, versions, _reader, _writer, receivedCallDispatcher);

        _logger = serverSettings.Logger;

        if (_port == 0)
        {
            if (_server.LocalEndPoint is not IPEndPoint localEndPoint)
            {
                throw new InvalidOperationException("Could not find local endpoint for server socket.");
            }

            _port = localEndPoint.Port;
        }

        if ((program != PortMapperConstants.PortMapperProgram) && (serverSettings.PortMapperPort != 0))
        {
            var clientSettings = new ClientSettings
            {
                Logger = serverSettings.Logger,
                ReceiveTimeout = serverSettings.ReceiveTimeout,
                SendTimeout = serverSettings.SendTimeout
            };
            foreach (int version in versions)
            {
                PortMapperUtilities.UnsetAndSetPort(
                    ipAddress.AddressFamily,
                    ProtocolKind.Udp,
                    serverSettings.PortMapperPort,
                    _port,
                    program,
                    version,
                    clientSettings);
            }
        }

        _logger?.Info($"{Utilities.ConvertToString(Protocol.Udp)} Server listening on {_server.LocalEndPoint}...");
    }

    public int Start()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(RpcUdpServer));
        }

        if (_receivingThread is not null)
        {
            return _port;
        }

        _receivingThread = new Thread(HandlingUdpCalls)
        {
            IsBackground = true,
            Name = $"RpcNet UDP Server Thread for Port {_port}"
        };
        _receivingThread.Start();
        return _port;
    }

    public void Dispose()
    {
        _stopReceiving = true;
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
        Interlocked.Exchange(ref _receivingThread, null)?.Join();
        _isDisposed = true;
    }

    private void HandlingUdpCalls()
    {
        while (!_stopReceiving)
        {
            try
            {
                IPEndPoint remoteIpEndPoint = _reader.BeginReading();

                _writer.BeginWriting();
                _receivedCall.HandleCall(new RpcEndPoint(remoteIpEndPoint, Protocol.Udp));
                _reader.EndReading();
                _writer.EndWriting(remoteIpEndPoint);
            }
            catch (RpcException e)
            {
                if (!_stopReceiving)
                {
                    _logger?.Error(e.Message);
                }
            }
            catch (Exception e)
            {
                if (!_stopReceiving)
                {
                    _logger?.Error($"The following error occurred while processing UDP call: {e}");
                }
            }
        }
    }
}
