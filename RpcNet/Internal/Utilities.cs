// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net.Sockets;
using System.Runtime.CompilerServices;

internal static class Utilities
{
    public static readonly TimeSpan DefaultClientReceiveTimeout = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan DefaultClientSendTimeout = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan DefaultServerReceiveTimeout = Timeout.InfiniteTimeSpan;
    public static readonly TimeSpan DefaultServerSendTimeout = TimeSpan.FromSeconds(10);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt32BigEndian(ReadOnlySpan<byte> value) =>
        (value[0] << 24) | (value[1] << 16) | (value[2] << 8) | value[3];

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

    public static string ConvertToString(Protocol protocol)
    {
        switch (protocol)
        {
            case Protocol.Tcp:
                return "TCP";
            case Protocol.Udp:
                return "UDP";
            case Protocol.TcpAndUdp:
                return "TCP and UDP";
            default:
                throw new ArgumentOutOfRangeException(nameof(protocol), protocol, null);
        }
    }

    public static TimeSpan GetReceiveTimeout(Socket socket)
    {
        try
        {
            return TimeSpan.FromMilliseconds(socket.ReceiveTimeout);
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not get receive timeout. Socket error: {e.SocketErrorCode}.");
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
            throw new RpcException(
                $"Could not set receive timeout to {timeout}. Socket error: {e.SocketErrorCode}.");
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
            throw new RpcException($"Could not get send timeout. Socket error: {e.SocketErrorCode}.");
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
            throw new RpcException($"Could not set send timeout to {timeout}. Socket error: {e.SocketErrorCode}.");
        }
    }
}
