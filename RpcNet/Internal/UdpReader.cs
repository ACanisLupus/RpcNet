// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;

internal sealed class UdpReader : INetworkReader
{
    private const int MaxUdpDatagramSize = 65536;

    private readonly byte[] _buffer = new byte[MaxUdpDatagramSize];
    private readonly Socket _socket;

    private int _length;
    private int _position;
    private EndPoint _remoteEndPoint;

    public UdpReader(Socket socket)
    {
        _socket = socket;
        IPAddress iPAddress = socket.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Loopback : IPAddress.Loopback;
        _remoteEndPoint = new IPEndPoint(iPAddress, 0);
    }

    public TimeSpan Timeout { get; set; }

    public async ValueTask<EndPoint> BeginReadingAsync(CancellationToken cancellationToken)
    {
        try
        {
            SocketReceiveFromResult result = await Utilities.ExecuteWithTimeoutAsync(
                    Timeout,
                    token => _socket.ReceiveFromAsync(_buffer, SocketFlags.None, _remoteEndPoint, token),
                    cancellationToken)
                .ConfigureAwait(false);
            _remoteEndPoint = result.RemoteEndPoint;
            _length = result.ReceivedBytes;
            _position = 0;
            return _remoteEndPoint;
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not receive data from UDP socket. Socket error code: {e.SocketErrorCode}.");
        }
    }

    public void EndReading() => _position = _length;

    public ReadOnlySpan<byte> Read(int length)
    {
        int available = _length - _position;
        if (length > available)
        {
            throw new RpcException("UDP buffer underflow.");
        }

        ReadOnlySpan<byte> span = _buffer.AsSpan(_position, length);
        _position += length;
        return span;
    }
}
