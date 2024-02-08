// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;

// Public for tests
public sealed class UdpReader : INetworkReader
{
    private readonly byte[] _buffer;
    private readonly Socket _udpClient;

    private int _readIndex;
    private EndPoint _remoteEndPoint;
    private int _totalLength;

    public UdpReader(Socket udpClient) : this(udpClient, 65536)
    {
    }

    public UdpReader(Socket udpClient, int bufferSize)
    {
        if ((bufferSize < sizeof(int)) || ((bufferSize % 4) != 0))
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }

        _udpClient = udpClient;
        IPAddress iPAddress = udpClient.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Loopback : IPAddress.Loopback;
        _remoteEndPoint = new IPEndPoint(iPAddress, 0);

        // See https://stackoverflow.com/questions/7201862/an-existing-connection-was-forcibly-closed-by-the-remote-host
        if (OperatingSystem.IsWindows())
        {
            const uint IocIn = 0x80000000;
            const uint IocVendor = 0x18000000;
            const uint SioUdpConnectionReset = IocIn | IocVendor | 12;
            _ = _udpClient.IOControl(
                unchecked((int)SioUdpConnectionReset),
                new[]
                {
                    Convert.ToByte(false)
                },
                null);
        }

        _buffer = new byte[bufferSize];
    }

    public IPEndPoint BeginReading()
    {
        _readIndex = 0;
        try
        {
            _totalLength = _udpClient.ReceiveFrom(_buffer, ref _remoteEndPoint);
            return (IPEndPoint)_remoteEndPoint;
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not receive data from UDP socket. Socket error code: {e.SocketErrorCode}.");
        }
    }

    public void EndReading()
    {
        // Just read to the end. Obviously, this is an unknown procedure
        if (_readIndex != _totalLength)
        {
            _ = Read(_totalLength - _readIndex);
        }
    }

    public ReadOnlySpan<byte> Read(int length)
    {
        if ((_readIndex + length) > _totalLength)
        {
            throw new RpcException("UDP buffer underflow.");
        }

        Span<byte> span = _buffer.AsSpan(_readIndex, length);
        _readIndex += length;
        return span;
    }
}
