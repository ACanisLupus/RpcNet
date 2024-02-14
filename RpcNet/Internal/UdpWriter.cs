// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;

// Public for tests
public sealed class UdpWriter : INetworkWriter
{
    private readonly byte[] _buffer;
    private readonly Socket _socket;

    private int _writeIndex;

    public UdpWriter(Socket socket) : this(socket, 65536)
    {
    }

    public UdpWriter(Socket socket, int bufferSize)
    {
        if ((bufferSize < sizeof(int)) || ((bufferSize % 4) != 0))
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }

        _socket = socket;
        _buffer = new byte[bufferSize];
    }

    public void BeginWriting() => _writeIndex = 0;

    public void EndWriting(EndPoint remoteEndPoint)
    {
        try
        {
            _ = _socket.SendTo(_buffer, _writeIndex, SocketFlags.None, remoteEndPoint);
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not send to {remoteEndPoint}. Socket error code: {e.SocketErrorCode}.");
        }
    }

    public Span<byte> Reserve(int length)
    {
        if ((_writeIndex + length) > _buffer.Length)
        {
            throw new RpcException("UDP buffer overflow.");
        }

        Span<byte> span = _buffer.AsSpan(_writeIndex, length);
        _writeIndex += length;
        return span;
    }
}
