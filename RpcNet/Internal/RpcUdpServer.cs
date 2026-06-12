// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using RpcNet.PortMapper;

internal sealed class RpcUdpServer : IDisposable
{
    private readonly IPAddress _ipAddress;
    private readonly int _program;
    private readonly Action<ReceivedRpcCall> _receivedCallDispatcher;
    private readonly Socket _socket;
    private readonly int[] _versions;
    private readonly ServerSettings _serverSettings;

    private bool _isDisposed;
    private int _port;
    private Thread? _receivingThread;
    private UdpReader? _reader;
    private ReceivedRpcCall? _receivedCall;
    private volatile bool _stopReceiving;
    private UdpWriter? _writer;

    public RpcUdpServer(
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
        _ipAddress = ipAddress;
        _port = port;
        _socket = new Socket(ipAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        Utilities.FixUdpSocket(_socket);

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

    public int Start()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, nameof(RpcUdpServer));

        if (_receivingThread is not null)
        {
            return _port;
        }

        try
        {
            _socket.Bind(new IPEndPoint(_ipAddress, _port));
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not start UDP listener. Socket error code: {e.SocketErrorCode}.");
        }

        if (_socket.LocalEndPoint is not IPEndPoint localEndPoint)
        {
            throw new InvalidOperationException("Could not find local endpoint for server socket.");
        }

        _reader = new UdpReader(_socket);
        _writer = new UdpWriter(_socket);
        _receivedCall = new ReceivedRpcCall(_program, _versions, _reader, _writer, _receivedCallDispatcher);

        if (_port == 0)
        {
            _port = localEndPoint.Port;
        }

        if ((_program != PortMapperConstants.PortMapperProgram) && (_serverSettings.PortMapperPort != 0))
        {
            ClientSettings clientSettings = new()
            {
                Logger = _serverSettings.Logger,
                ReceiveTimeout = _serverSettings.ReceiveTimeout,
                SendTimeout = _serverSettings.SendTimeout
            };
            foreach (int version in _versions)
            {
                PortMapperUtilities.UnsetAndSetPort(
                    _ipAddress.AddressFamily,
                    ProtocolKind.Udp,
                    _serverSettings.PortMapperPort,
                    _port,
                    _program,
                    version,
                    clientSettings);
            }
        }

        _serverSettings.Logger?.Info($"UDP Server listening on {localEndPoint}...");

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
            _socket.Shutdown(SocketShutdown.Both);
        }
        catch
        {
            // Ignored
        }

        _socket.Dispose();
        Interlocked.Exchange(ref _receivingThread, null)?.Join();
        _isDisposed = true;
    }

    private void HandlingUdpCalls()
    {
        while (!_stopReceiving)
        {
            try
            {
                EndPoint remoteEndPoint = _reader!.BeginReading();

                _writer!.BeginWriting();
                _receivedCall!.HandleCall(new RpcEndPoint(remoteEndPoint, Protocol.Udp));
                _reader!.EndReading();
                _writer!.EndWriting(remoteEndPoint);
            }
            catch (RpcException e)
            {
                if (!_stopReceiving)
                {
                    _serverSettings.Logger?.Error(e.Message);
                }
            }
            catch (Exception e)
            {
                if (!_stopReceiving)
                {
                    _serverSettings.Logger?.Error($"The following error occurred while processing UDP call: {e}");
                }
            }
        }
    }
}
