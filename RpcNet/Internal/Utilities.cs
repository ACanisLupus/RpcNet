// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

internal static class Utilities
{
    public const uint RpcVersion = 2;

    public static readonly TimeSpan DefaultClientReceiveTimeout = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan DefaultClientSendTimeout = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan DefaultServerReceiveTimeout = Timeout.InfiniteTimeSpan;
    public static readonly TimeSpan DefaultServerSendTimeout = TimeSpan.FromSeconds(10);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt32BigEndian(ReadOnlySpan<byte> value) => (value[0] << 24) | (value[1] << 16) | (value[2] << 8) | value[3];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteBytesBigEndian(Span<byte> destination, int value)
    {
        destination[0] = (byte)(value >> 24);
        destination[1] = (byte)((value >> 16) & 0xff);
        destination[2] = (byte)((value >> 8) & 0xff);
        destination[3] = (byte)(value & 0xff);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CalculateXdrPadding(int length) => (4 - (length & 3)) & 3;

    public static IPAddress GetAlternateIpAddress(IPAddress ipAddress)
    {
        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // Try again with IPv4
            return IPAddress.IsLoopback(ipAddress) ? IPAddress.Loopback : ipAddress.MapToIPv4();
        }

        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            // Try again with IPv6
            return IPAddress.IsLoopback(ipAddress) ? IPAddress.IPv6Loopback : ipAddress.MapToIPv6();
        }

        throw new InvalidOperationException($"The following address family is unsupported: {ipAddress.AddressFamily}.");
    }

    public static IPAddress GetLoopbackAddress(AddressFamily addressFamily)
    {
        if (addressFamily == AddressFamily.InterNetworkV6)
        {
            return IPAddress.IPv6Loopback;
        }

        if (addressFamily == AddressFamily.InterNetwork)
        {
            return IPAddress.Loopback;
        }

        throw new InvalidOperationException($"The following address family is unsupported: {addressFamily}.");
    }

    public static string ConvertToString(Protocol protocol) =>
        protocol switch
        {
            Protocol.Tcp => "TCP",
            Protocol.Udp => "UDP",
            Protocol.TcpAndUdp => "TCP and UDP",
            _ => throw new ArgumentOutOfRangeException(nameof(protocol), protocol, null)
        };

    public static TimeSpan GetReceiveTimeout(Socket socket)
    {
        try
        {
            return TimeSpan.FromMilliseconds(socket.ReceiveTimeout);
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not get receive timeout. Socket error code: {e.SocketErrorCode}.");
        }
    }

    public static void SetReceiveTimeout(Socket socket, TimeSpan timeout)
    {
        try
        {
            socket.ReceiveTimeout = (int)timeout.TotalMilliseconds;
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not set receive timeout to {timeout}. Socket error code: {e.SocketErrorCode}.");
        }
    }

    public static TimeSpan GetSendTimeout(Socket socket)
    {
        try
        {
            return TimeSpan.FromMilliseconds(socket.SendTimeout);
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not get send timeout. Socket error code: {e.SocketErrorCode}.");
        }
    }

    public static void SetSendTimeout(Socket socket, TimeSpan timeout)
    {
        try
        {
            socket.SendTimeout = (int)timeout.TotalMilliseconds;
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not set send timeout to {timeout}. Socket error code: {e.SocketErrorCode}.");
        }
    }
}
