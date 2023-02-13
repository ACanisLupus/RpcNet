// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using PortMapper;

// Public for tests
public class RpcUdpServer : IDisposable
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
        _server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
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

            if (program != PortMapperConstants.PortMapperProgram)
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
                        ProtocolKind.Udp,
                        serverSettings.PortMapperPort,
                        _port,
                        program,
                        version,
                        clientSettings);
                }
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

        if (_receivingThread != null)
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
                NetworkReadResult result = _reader.BeginReading();
                if (result.HasError)
                {
                    _logger?.Trace($"Could not read UDP data. Socket error: {result.SocketError}.");
                    continue;
                }

                if (result.IsDisconnected)
                {
                    // Should only happen on dispose
                    continue;
                }

                IPEndPoint? remoteIpEndPoint = result.RemoteIpEndPoint;
                if (remoteIpEndPoint == null)
                {
                    // Can this happen?
                    continue;
                }

                _writer.BeginWriting();
                _receivedCall.HandleCall(new Caller(remoteIpEndPoint, Protocol.Udp));
                _reader.EndReading();
                _writer.EndWriting(remoteIpEndPoint);
            }
            catch (Exception e)
            {
                _logger?.Error($"The following error occurred while processing UDP call: {e}");
            }
        }
    }
}
