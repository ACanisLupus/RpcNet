// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;

// Public for tests
public sealed class TcpWriter : INetworkWriter
{
    private const int TcpHeaderLength = 4;

    private readonly byte[] _buffer;
    private readonly Socket _tcpClient;

    private int _writeIndex;

    public TcpWriter(Socket tcpClient) : this(tcpClient, 65536)
    {
    }

    public TcpWriter(Socket tcpClient, int bufferSize)
    {
        if ((bufferSize < (TcpHeaderLength + sizeof(int))) || ((bufferSize % 4) != 0))
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }

        _tcpClient = tcpClient;
        _buffer = new byte[bufferSize];
    }

    public void BeginWriting() => _writeIndex = TcpHeaderLength;
    public void EndWriting(IPEndPoint remoteEndPoint) => FlushPacket(true);

    public Span<byte> Reserve(int length)
    {
        int maxLength = _buffer.Length - _writeIndex;

        // Integers (4 bytes) and padding bytes (> 1 and < 4 bytes) must not be sent fragmented
        if ((maxLength < length) && (maxLength < sizeof(int)))
        {
            FlushPacket(false);
            maxLength = _buffer.Length - _writeIndex;
        }

        int reservedLength = Math.Min(length, maxLength);

        Span<byte> span = _buffer.AsSpan(_writeIndex, reservedLength);
        _writeIndex += reservedLength;
        return span;
    }

    private void FlushPacket(bool lastPacket)
    {
        int length = _writeIndex - TcpHeaderLength;

        // Last fragment sets first bit to 1
        int lengthToDecode = lastPacket ? length | unchecked((int)0x80000000) : length;

        Utilities.WriteBytesBigEndian(_buffer.AsSpan(), lengthToDecode);

        SocketError socketError;
        try
        {
            _ = _tcpClient.Send(_buffer, 0, length + TcpHeaderLength, SocketFlags.None, out socketError);
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not send data to {_tcpClient.RemoteEndPoint}. Socket error code: {e.SocketErrorCode}.");
        }

        if (socketError == SocketError.Success)
        {
            BeginWriting();
        }
    }
}
