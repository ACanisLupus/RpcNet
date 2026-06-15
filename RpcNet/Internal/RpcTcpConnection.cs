// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;

internal sealed class RpcTcpConnection : IDisposable
{
    private readonly ILogger? _logger;
    private readonly TcpReader _reader;
    private readonly ReceivedRpcCall _receivedCall;
    private readonly EndPoint _remoteEndPoint;
    private readonly RpcEndPoint _rpcEndPoint;
    private readonly Socket _socket;
    private readonly TcpWriter _writer;

    public RpcTcpConnection(
        Socket socket,
        int program,
        int[] versions,
        Func<ReceivedRpcCall, CancellationToken, ValueTask> receivedCallDispatcher,
        ILogger? logger = null)
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

    public async ValueTask<bool> HandleAsync(CancellationToken cancellationToken)
    {
        try
        {
            _ = await _reader.BeginReadingAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (RpcException e)
        {
            _logger?.Trace(e.Message);
            return false;
        }

        _writer.BeginWriting();
        await _receivedCall.HandleCallAsync(_rpcEndPoint, cancellationToken).ConfigureAwait(false);
        _reader.EndReading();

        try
        {
            await _writer.EndWritingAsync(_remoteEndPoint, cancellationToken).ConfigureAwait(false);
        }
        catch (RpcException e)
        {
            _logger?.Trace(e.Message);
            return false;
        }

        return true;
    }
}
