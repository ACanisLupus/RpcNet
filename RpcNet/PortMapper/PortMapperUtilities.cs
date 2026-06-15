// Copyright by Artur Wolf

namespace RpcNet.PortMapper;

using System.Net;
using System.Net.Sockets;
using RpcNet.Internal;

public static class PortMapperUtilities
{
    public static async ValueTask<int> GetPortAsync(
        ProtocolKind protocol,
        IPAddress ipAddress,
        int portMapperPort,
        int program,
        int version,
        ClientSettings? clientSettings,
        CancellationToken cancellationToken)
    {
        try
        {
            return await GetPortInternalAsync(protocol, ipAddress, portMapperPort, program, version, clientSettings, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return await GetPortInternalAsync(
                    protocol,
                    Utilities.GetAlternateIpAddress(ipAddress),
                    portMapperPort,
                    program,
                    version,
                    clientSettings,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public static async ValueTask UnsetAndSetPortAsync(
        AddressFamily addressFamily,
        ProtocolKind protocol,
        int portMapperPort,
        int portToSet,
        int program,
        int version,
        ClientSettings? clientSettings,
        CancellationToken cancellationToken)
    {
        IPAddress ipAddress = Utilities.GetLoopbackAddress(addressFamily);
        try
        {
            await UnsetAndSetPortInternalAsync(ipAddress, protocol, portMapperPort, portToSet, program, version, clientSettings, cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            await UnsetAndSetPortInternalAsync(
                    Utilities.GetAlternateIpAddress(ipAddress),
                    protocol,
                    portMapperPort,
                    portToSet,
                    program,
                    version,
                    clientSettings,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static async ValueTask<int> GetPortInternalAsync(
        ProtocolKind protocol,
        IPAddress ipAddress,
        int portMapperPort,
        int program,
        int version,
        ClientSettings? clientSettings,
        CancellationToken cancellationToken)
    {
        using PortMapperClient portMapperClient =
            await PortMapperClient.ConnectAsync(GetPortMapperProtocol(protocol), ipAddress, portMapperPort, clientSettings, cancellationToken)
                .ConfigureAwait(false);
        return await portMapperClient.GetPort_2Async(
                new Mapping2
                {
                    ProgramNumber = program,
                    Protocol = protocol,
                    VersionNumber = version
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static async ValueTask UnsetAndSetPortInternalAsync(
        IPAddress ipAddress,
        ProtocolKind protocol,
        int portMapperPort,
        int portToSet,
        int program,
        int version,
        ClientSettings? clientSettings,
        CancellationToken cancellationToken)
    {
        using PortMapperClient portMapperClient =
            await PortMapperClient.ConnectAsync(GetPortMapperProtocol(protocol), ipAddress, portMapperPort, clientSettings, cancellationToken)
                .ConfigureAwait(false);
        _ = await portMapperClient.Unset_2Async(
                new Mapping2
                {
                    ProgramNumber = program,
                    Protocol = protocol,
                    VersionNumber = version
                },
                cancellationToken)
            .ConfigureAwait(false);
        _ = await portMapperClient.Set_2Async(
                new Mapping2
                {
                    Port = portToSet,
                    ProgramNumber = program,
                    Protocol = protocol,
                    VersionNumber = version
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static Protocol GetPortMapperProtocol(ProtocolKind protocol) => protocol switch
    {
        ProtocolKind.Tcp => Protocol.Tcp,
        ProtocolKind.Udp => Protocol.Udp,
        _ => throw new ArgumentOutOfRangeException(nameof(protocol))
    };
}
