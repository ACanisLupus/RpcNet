// Copyright by Artur Wolf

namespace RpcNet.PortMapper;

using System.Net;

internal static class PortMapperUtilities
{
    public static int GetPort(
        ProtocolKind protocol,
        IPAddress ipAddress,
        int portMapperPort,
        int program,
        int version,
        ClientSettings clientSettings = default)
    {
        using var portMapperClient = new PortMapperClient(Protocol.Tcp, ipAddress, portMapperPort, clientSettings);
        return portMapperClient.GetPort_2(
            new Mapping
            {
                Program = program,
                Protocol = protocol,
                Version = version
            });
    }

    public static void UnsetAndSetPort(
        ProtocolKind protocol,
        int portMapperPort,
        int portToSet,
        int program,
        int version,
        ClientSettings clientSettings = default)
    {
        using var portMapperClient = new PortMapperClient(
            Protocol.Tcp,
            IPAddress.Loopback,
            portMapperPort,
            clientSettings);
        portMapperClient.Unset_2(
            new Mapping
            {
                Program = program,
                Protocol = protocol,
                Version = version
            });
        portMapperClient.Set_2(
            new Mapping
            {
                Port = portToSet,
                Program = program,
                Protocol = protocol,
                Version = version
            });
    }
}
