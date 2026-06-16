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

    public static IPAddress GetAlternateIpAddress(IPAddress ipAddress) =>
        ipAddress.AddressFamily switch
        {
            AddressFamily.InterNetworkV6 => IPAddress.IsLoopback(ipAddress) ? IPAddress.Loopback : ipAddress.MapToIPv4(),
            AddressFamily.InterNetwork => IPAddress.IsLoopback(ipAddress) ? IPAddress.IPv6Loopback : ipAddress.MapToIPv6(),
            _ => throw new InvalidOperationException($"The following address family is unsupported: {ipAddress.AddressFamily}.")
        };

    public static IPAddress GetLoopbackAddress(AddressFamily addressFamily) =>
        addressFamily switch
        {
            AddressFamily.InterNetworkV6 => IPAddress.IPv6Loopback,
            AddressFamily.InterNetwork => IPAddress.Loopback,
            _ => throw new InvalidOperationException($"The following address family is unsupported: {addressFamily}.")
        };

    public static async ValueTask<TResult> ExecuteWithTimeoutAsync<TResult>(
        TimeSpan timeout,
        Func<CancellationToken, ValueTask<TResult>> operation,
        CancellationToken cancellationToken)
    {
        // Socket.ReceiveTimeout/SendTimeout only affect synchronous socket calls and are ignored by the
        // asynchronous socket APIs used throughout RpcNet. Enforce the timeout with a linked cancellation
        // token instead. A non-positive timeout (e.g. Timeout.InfiniteTimeSpan) means "wait indefinitely".
        if (timeout <= TimeSpan.Zero)
        {
            return await operation(cancellationToken).ConfigureAwait(false);
        }

        using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);
        try
        {
            return await operation(timeoutSource.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new RpcException($"The operation did not complete within the configured timeout of {timeout}.");
        }
    }

    public static void FixUdpSocket(Socket socket)
    {
        // See https://stackoverflow.com/questions/7201862/an-existing-connection-was-forcibly-closed-by-the-remote-host
        if (OperatingSystem.IsWindows())
        {
            const uint IocIn = 0x80000000;
            const uint IocVendor = 0x18000000;
            const uint SioUdpConnectionReset = IocIn | IocVendor | 12;
            _ = socket.IOControl(
                unchecked((int)SioUdpConnectionReset),
                [
                    Convert.ToByte(false)
                ],
                null);
        }
    }
}
