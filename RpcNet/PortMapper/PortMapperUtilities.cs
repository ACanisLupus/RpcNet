// Copyright by Artur Wolf

namespace RpcNet.PortMapper;

using System.Net;

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
            UnsetAndSetPort(IPAddress.Loopback, protocol, portMapperPort, portToSet, program, version, clientSettings);
        }
        catch
        {
            UnsetAndSetPort(IPAddress.IPv6Loopback, protocol, portMapperPort, portToSet, program, version, clientSettings);
        }
    }

    private static void UnsetAndSetPort(
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
