// Copyright by Artur Wolf

namespace RpcNet.PortMapper;

using System.Net;
using System.Net.Sockets;
using RpcNet.Internal;

public static class PortMapperUtilities
{
    public static int GetPort(
        ProtocolKind protocol,
        IPAddress ipAddress,
        int portMapperPort,
        int program,
        int version,
        ClientSettings? clientSettings)
    {
        try
        {
            return GetPortInternal(protocol, ipAddress, portMapperPort, program, version, clientSettings);
        }
        catch
        {
            return GetPortInternal(protocol, Utilities.GetAlternateIpAddress(ipAddress), portMapperPort, program, version, clientSettings);
        }
    }

    public static int GetPortInternal(
        ProtocolKind protocol,
        IPAddress ipAddress,
        int portMapperPort,
        int program,
        int version,
        ClientSettings? clientSettings)
    {
        using var portMapperClient = new PortMapperClient(Protocol.Tcp, ipAddress, portMapperPort, clientSettings);
        return portMapperClient.GetPort_2(new Mapping2 { ProgramNumber = program, Protocol = protocol, VersionNumber = version });
    }

    public static void UnsetAndSetPort(
        ProtocolKind protocol,
        int portMapperPort,
        int portToSet,
        int program,
        int version,
        ClientSettings? clientSettings)
    {
        try
        {
            UnsetAndSetPortInternal(IPAddress.Loopback, protocol, portMapperPort, portToSet, program, version, clientSettings);
        }
        catch
        {
            UnsetAndSetPortInternal(IPAddress.IPv6Loopback, protocol, portMapperPort, portToSet, program, version, clientSettings);
        }
    }

    private static void UnsetAndSetPortInternal(
        IPAddress ipAddress,
        ProtocolKind protocol,
        int portMapperPort,
        int portToSet,
        int program,
        int version,
        ClientSettings? clientSettings)
    {
        using var portMapperClient = new PortMapperClient(Protocol.Tcp, ipAddress, portMapperPort, clientSettings);
        portMapperClient.Unset_2(new Mapping2 { ProgramNumber = program, Protocol = protocol, VersionNumber = version });
        portMapperClient.Set_2(new Mapping2 { Port = portToSet, ProgramNumber = program, Protocol = protocol, VersionNumber = version });
    }
}
