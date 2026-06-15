// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;

internal sealed class TcpWriter(Socket socket) : INetworkWriter
{
    private const int TcpHeaderLength = 4;

    private readonly MemoryStream _buffer = new();

    public TimeSpan Timeout { get; set; }

    public void BeginWriting()
    {
        _buffer.SetLength(TcpHeaderLength);
        _buffer.Position = TcpHeaderLength;
    }

    public async ValueTask EndWritingAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        int length = (int)_buffer.Length - TcpHeaderLength;
        int lengthToDecode = length | unchecked((int)0x80000000);

        Utilities.WriteBytesBigEndian(_buffer.GetBuffer().AsSpan(), lengthToDecode);

        try
        {
            _ = await Utilities.ExecuteWithTimeoutAsync(
                    Timeout,
                    token => socket.SendAsync(_buffer.GetBuffer().AsMemory(0, (int)_buffer.Length), SocketFlags.None, token),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not send data to {socket.RemoteEndPoint}. Socket error code: {e.SocketErrorCode}.");
        }

        BeginWriting();
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
