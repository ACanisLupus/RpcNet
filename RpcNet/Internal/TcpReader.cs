// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;

internal sealed class TcpReader(Socket socket) : INetworkReader
{
    private const int NetworkBufferSize = 65536;
    private const int TcpHeaderLength = 4;

    private readonly MemoryStream _buffer = new();
    private readonly byte[] _networkBuffer = new byte[NetworkBufferSize];

    private int _networkReadPos;
    private int _networkWritePos;

    public TimeSpan Timeout { get; set; }

    public async ValueTask<EndPoint> BeginReadingAsync(CancellationToken cancellationToken)
    {
        _buffer.SetLength(0);
        _buffer.Position = 0;

        byte[] headerBuf = new byte[TcpHeaderLength];
        bool lastFragment = false;
        while (!lastFragment)
        {
            await ConsumeExactAsync(headerBuf, cancellationToken).ConfigureAwait(false);
            int headerValue = Utilities.ToInt32BigEndian(headerBuf);
            lastFragment = headerValue < 0;
            int fragmentLength = headerValue & 0x0fffffff;

            if (((fragmentLength % 4) != 0) || (fragmentLength == 0))
            {
                throw new RpcException("This is not an XDR stream.");
            }

            long offset = _buffer.Length;
            _buffer.SetLength(offset + fragmentLength);
            await ConsumeExactAsync(_buffer.GetBuffer(), (int)offset, fragmentLength, cancellationToken).ConfigureAwait(false);
        }

        _buffer.Position = 0;
        return socket.RemoteEndPoint!;
    }

    public void EndReading() => _buffer.Position = _buffer.Length;

    public ReadOnlySpan<byte> Read(int length)
    {
        int available = (int)(_buffer.Length - _buffer.Position);
        int toRead = Math.Min(available, length);
        ReadOnlySpan<byte> span = _buffer.GetBuffer().AsSpan((int)_buffer.Position, toRead);
        _buffer.Position += toRead;
        return span;
    }

    private async ValueTask ConsumeExactAsync(byte[] target, CancellationToken cancellationToken) =>
        await ConsumeExactAsync(target, 0, target.Length, cancellationToken).ConfigureAwait(false);

    private async ValueTask ConsumeExactAsync(byte[] target, int offset, int count, CancellationToken cancellationToken)
    {
        int consumed = 0;
        while (consumed < count)
        {
            int buffered = _networkWritePos - _networkReadPos;
            if (buffered == 0)
            {
                await FillNetworkBufferAsync(cancellationToken).ConfigureAwait(false);
                continue;
            }

            int toCopy = Math.Min(count - consumed, buffered);
            Array.Copy(_networkBuffer, _networkReadPos, target, offset + consumed, toCopy);
            _networkReadPos += toCopy;
            consumed += toCopy;
        }
    }

    private async ValueTask FillNetworkBufferAsync(CancellationToken cancellationToken)
    {
        int remaining = _networkWritePos - _networkReadPos;
        if ((remaining > 0) && (_networkReadPos > 0))
        {
            _networkBuffer.AsSpan(_networkReadPos, remaining).CopyTo(_networkBuffer);
        }

        _networkWritePos = remaining;
        _networkReadPos = 0;

        int n;
        try
        {
            n = await Utilities.ExecuteWithTimeoutAsync(
                    Timeout,
                    token => socket.ReceiveAsync(_networkBuffer.AsMemory(_networkWritePos), SocketFlags.None, token),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not read from {socket.RemoteEndPoint}. Socket error code: {e.SocketErrorCode}.");
        }

        if (n == 0)
        {
            throw new RpcException($"{socket.RemoteEndPoint} disconnected.");
        }

        _networkWritePos += n;
    }
}
