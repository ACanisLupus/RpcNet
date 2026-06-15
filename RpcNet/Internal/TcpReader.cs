// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;

internal sealed class TcpReader(Socket socket) : INetworkReader
{
    private const int FragmentLengthMask = 0x7fffffff;
    private const int LastFragmentFlag = unchecked((int)0x80000000);
    private const int NetworkBufferSize = 65536;
    private const int TcpHeaderLength = 4;

    private readonly MemoryStream _buffer = new();
    private readonly byte[] _headerBuffer = new byte[TcpHeaderLength];
    private readonly byte[] _networkBuffer = new byte[NetworkBufferSize];

    private int _networkReadPos;
    private int _networkWritePos;

    public TimeSpan Timeout { get; set; }

    public async ValueTask<EndPoint> BeginReadingAsync(CancellationToken cancellationToken)
    {
        _buffer.SetLength(0);
        _buffer.Position = 0;

        bool lastFragment = false;
        while (!lastFragment)
        {
            await ConsumeExactAsync(_headerBuffer, cancellationToken).ConfigureAwait(false);
            int headerValue = Utilities.ToInt32BigEndian(_headerBuffer);
            lastFragment = (headerValue & LastFragmentFlag) != 0;
            int fragmentLength = headerValue & FragmentLengthMask;

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
        if (length > available)
        {
            throw new RpcException("TCP buffer underflow.");
        }

        ReadOnlySpan<byte> span = _buffer.GetBuffer().AsSpan((int)_buffer.Position, length);
        _buffer.Position += length;
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
        // This is only called once the buffered data has been fully consumed, so the network buffer
        // can always be refilled from the beginning.
        _networkReadPos = 0;
        _networkWritePos = 0;

        int n;
        try
        {
            n = await Utilities.ExecuteWithTimeoutAsync(
                    Timeout,
                    token => socket.ReceiveAsync(_networkBuffer.AsMemory(), SocketFlags.None, token),
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
