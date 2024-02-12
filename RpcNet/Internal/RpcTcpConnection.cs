// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;

// Public for tests
public sealed class RpcTcpConnection : IDisposable
{
    private readonly Caller _caller;
    private readonly ILogger? _logger;
    private readonly TcpReader _reader;
    private readonly ReceivedRpcCall _receivedCall;
    private readonly IPEndPoint _remoteIpEndPoint;
    private readonly Socket _tcpClient;
    private readonly TcpWriter _writer;

    public RpcTcpConnection(Socket tcpClient, int program, int[] versions, Action<ReceivedRpcCall> receivedCallDispatcher, ILogger? logger = default)
    {
        _tcpClient = tcpClient;
        if (tcpClient.RemoteEndPoint is not IPEndPoint remoteIpEndPoint)
        {
            throw new RpcException("Could not find remote IP end point for TCP connection.");
        }

        _remoteIpEndPoint = remoteIpEndPoint;
        _caller = new Caller(remoteIpEndPoint, Protocol.Tcp);
        _reader = new TcpReader(tcpClient);
        _writer = new TcpWriter(tcpClient);
        _logger = logger;

        _receivedCall = new ReceivedRpcCall(program, versions, _reader, _writer, receivedCallDispatcher);

        _logger?.Trace($"{remoteIpEndPoint} connected.");
    }

    public void Dispose() => _tcpClient.Dispose();

    public bool Handle()
    {
        try
        {
            _ = _reader.BeginReading();
        }
        catch (RpcException e)
        {
            _logger?.Trace(e.Message);
            return false;
        }

        _writer.BeginWriting();
        _receivedCall.HandleCall(_caller);
        _reader.EndReading();

        try
        {
            _writer.EndWriting(_remoteIpEndPoint);
        }
        catch (RpcException e)
        {
            _logger?.Trace(e.Message);
            return false;
        }

        return true;
    }
}
