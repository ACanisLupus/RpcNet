// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;

// Public for tests
public sealed class RpcTcpConnection : IDisposable
{
    private readonly ILogger? _logger;
    private readonly TcpReader _reader;
    private readonly ReceivedRpcCall _receivedCall;
    private readonly EndPoint _remoteEndPoint;
    private readonly RpcEndPoint _rpcEndPoint;
    private readonly Socket _socket;
    private readonly TcpWriter _writer;

    public RpcTcpConnection(Socket socket, int program, int[] versions, Action<ReceivedRpcCall> receivedCallDispatcher, ILogger? logger = default)
    {
        _socket = socket;
        if (socket.RemoteEndPoint is not IPEndPoint remoteIpEndPoint)
        {
            throw new RpcException("Could not find remote IP end point for TCP connection.");
        }

        _remoteEndPoint = remoteIpEndPoint;
        _rpcEndPoint = new RpcEndPoint(remoteIpEndPoint, Protocol.Tcp);
        _reader = new TcpReader(socket);
        _writer = new TcpWriter(socket);
        _logger = logger;

        _receivedCall = new ReceivedRpcCall(program, versions, _reader, _writer, receivedCallDispatcher);

        _logger?.Trace($"{remoteIpEndPoint} connected.");
    }

    public void Dispose() => _socket.Dispose();

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
        _receivedCall.HandleCall(_rpcEndPoint);
        _reader.EndReading();

        try
        {
            _writer.EndWriting(_remoteEndPoint);
        }
        catch (RpcException e)
        {
            _logger?.Trace(e.Message);
            return false;
        }

        return true;
    }
}
