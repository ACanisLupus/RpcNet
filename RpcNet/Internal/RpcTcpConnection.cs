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
        _reader = new TcpReader(tcpClient, logger);
        _writer = new TcpWriter(tcpClient, logger);
        _logger = logger;

        _receivedCall = new ReceivedRpcCall(program, versions, _reader, _writer, receivedCallDispatcher);

        _logger?.Trace($"{_caller} connected.");
    }

    public void Dispose() => _tcpClient.Dispose();

    public bool Handle()
    {
        NetworkReadResult readResult = _reader.BeginReading();
        if (readResult.HasError)
        {
            _logger?.Trace($"Could not read data from {_caller}. Socket error: {readResult.SocketError}.");
            return false;
        }

        if (readResult.IsDisconnected)
        {
            _logger?.Trace($"{_caller} disconnected.");
            return false;
        }

        _writer.BeginWriting();
        _receivedCall.HandleCall(_caller);
        _reader.EndReading();

        NetworkWriteResult writeResult = _writer.EndWriting(_remoteIpEndPoint);
        if (writeResult.HasError)
        {
            _logger?.Trace($"Could not write data to {_caller}. Socket error: {writeResult.SocketError}.");
            return false;
        }

        return true;
    }
}
