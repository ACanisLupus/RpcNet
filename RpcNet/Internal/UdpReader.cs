// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;

internal sealed class UdpReader : INetworkReader
{
    private readonly MemoryStream _buffer = new();
    private readonly Socket _socket;

    private EndPoint _remoteEndPoint;

    public UdpReader(Socket socket)
    {
        _socket = socket;
        IPAddress iPAddress = socket.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Loopback : IPAddress.Loopback;
        _remoteEndPoint = new IPEndPoint(iPAddress, 0);
    }

    public async ValueTask<EndPoint> BeginReadingAsync(CancellationToken cancellationToken)
    {
        try
        {
            byte[] temp = new byte[65536];
            SocketReceiveFromResult result = await _socket
                .ReceiveFromAsync(temp, SocketFlags.None, _remoteEndPoint, cancellationToken)
                .ConfigureAwait(false);
            int received = result.ReceivedBytes;
            _remoteEndPoint = result.RemoteEndPoint;
            _buffer.SetLength(received);
            _buffer.Position = 0;
            temp.AsSpan(0, received).CopyTo(_buffer.GetBuffer());
            return _remoteEndPoint;
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not receive data from UDP socket. Socket error code: {e.SocketErrorCode}.");
        }
    }

    public void EndReading() => _buffer.Position = _buffer.Length;

    public ReadOnlySpan<byte> Read(int length)
    {
        int available = (int)(_buffer.Length - _buffer.Position);
        if (length > available)
        {
            throw new RpcException("UDP buffer underflow.");
        }

        ReadOnlySpan<byte> span = _buffer.GetBuffer().AsSpan((int)_buffer.Position, length);
        _buffer.Position += length;
        return span;
    }
}
