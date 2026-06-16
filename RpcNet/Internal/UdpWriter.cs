// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;

internal sealed class UdpWriter(Socket socket) : INetworkWriter
{
    private readonly MemoryStream _buffer = new();

    public TimeSpan Timeout { get; set; }

    public void BeginWriting()
    {
        _buffer.SetLength(0);
        _buffer.Position = 0;
    }

    public async ValueTask EndWritingAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        try
        {
            _ = await Utilities.ExecuteWithTimeoutAsync(
                    Timeout,
                    token => socket.SendToAsync(_buffer.GetBuffer().AsMemory(0, (int)_buffer.Length), SocketFlags.None, remoteEndPoint, token),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not send to {remoteEndPoint}. Socket error code: {e.SocketErrorCode}.");
        }
    }

    public Span<byte> Reserve(int length)
    {
        int startPosition = (int)_buffer.Position;
        long newLength = startPosition + length;

        if (newLength > _buffer.Length)
        {
            _buffer.SetLength(newLength);
        }

        _buffer.Position = newLength;
        return _buffer.GetBuffer().AsSpan(startPosition, length);
    }
}
