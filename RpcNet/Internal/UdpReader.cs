// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;

// Public for tests
public class UdpReader : INetworkReader
{
    private readonly byte[] _buffer;
    private readonly Socket _udpClient;

    private int _readIndex;
    private EndPoint _remoteEndPoint = new IPEndPoint(IPAddress.Loopback, 0);
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

        // See
        // https://stackoverflow.com/questions/7201862/an-existing-connection-was-forcibly-closed-by-the-remote-host
        if (OperatingSystem.IsWindows())
        {
            const uint IocIn = 0x80000000;
            const uint IocVendor = 0x18000000;
            const uint SioUdpConnectionReset = IocIn | IocVendor | 12;
            _udpClient.IOControl(unchecked((int)SioUdpConnectionReset), new[] { Convert.ToByte(false) }, null);
        }

        _buffer = new byte[bufferSize];
    }

    public NetworkReadResult BeginReading()
    {
        _readIndex = 0;
        try
        {
            _totalLength = _udpClient.ReceiveFrom(_buffer, SocketFlags.None, ref _remoteEndPoint);
            if (_totalLength == 0)
            {
                return NetworkReadResult.CreateDisconnected();
            }

            return NetworkReadResult.CreateSuccess((IPEndPoint)_remoteEndPoint);
        }
        catch (SocketException e)
        {
            return NetworkReadResult.CreateError(e.SocketErrorCode);
        }
    }

    public void EndReading()
    {
        if (_readIndex != _totalLength)
        {
            throw new RpcException("Not all UDP data was read.");
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
