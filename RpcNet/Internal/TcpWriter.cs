// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;

// Public for tests
public class TcpWriter : INetworkWriter
{
    private const int TcpHeaderLength = 4;

    private readonly byte[] _buffer;
    private readonly ILogger _logger;

    private Socket _tcpClient;
    private int _writeIndex;

    public TcpWriter(Socket tcpClient, ILogger logger = default) : this(tcpClient, 65536, logger)
    {
    }

    public TcpWriter(Socket tcpClient, int bufferSize, ILogger logger = default)
    {
        if ((bufferSize < (TcpHeaderLength + sizeof(int))) || ((bufferSize % 4) != 0))
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }

        _logger = logger;

        Reset(tcpClient);
        _buffer = new byte[bufferSize];
    }

    public void Reset(Socket tcpClient) => _tcpClient = tcpClient;
    public void BeginWriting() => _writeIndex = TcpHeaderLength;
    public NetworkWriteResult EndWriting(IPEndPoint remoteEndPoint) => FlushPacket(true);

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

    private NetworkWriteResult FlushPacket(bool lastPacket)
    {
        int length = _writeIndex - TcpHeaderLength;

        // Last fragment sets first bit to 1
        int lengthToDecode = lastPacket ? length | unchecked((int)0x80000000) : length;

        Utilities.WriteBytesBigEndian(_buffer.AsSpan(), lengthToDecode);

        SocketError socketError;
        try
        {
            _tcpClient.Send(_buffer, 0, length + TcpHeaderLength, SocketFlags.None, out socketError);
        }
        catch (SocketException exception)
        {
            return new NetworkWriteResult(exception.SocketErrorCode);
        }
        catch (Exception exception)
        {
            _logger?.Error($"Unexpected error while sending TCP data to {_tcpClient?.RemoteEndPoint}: {exception}");
            return new NetworkWriteResult(SocketError.SocketError);
        }

        if (socketError == SocketError.Success)
        {
            BeginWriting();
        }

        return new NetworkWriteResult(socketError);
    }
}
