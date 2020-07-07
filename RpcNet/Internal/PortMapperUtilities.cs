namespace RpcNet.Internal
{
    using System.Net;

    internal static class PortMapperUtilities
    {
        public static int GetPort(ProtocolKind protocol, IPAddress ipAddress, int program, int version)
        {
            using (var portMapperClient = new PortMapperClient(
                Protocol.Tcp,
                ipAddress,
                PortMapperConstants.PortMapperPort))
            {
                return portMapperClient.GetPort_2(
                    new Mapping
                    {
                        Program = program,
                        Protocol = protocol,
                        Version = version
                    });
            }
        }

        public static void UnsetAndSetPort(ProtocolKind protocol, int port, int program, int version)
        {
            using (var portMapperClient = new PortMapperClient(
                Protocol.Tcp,
                IPAddress.Loopback,
                PortMapperConstants.PortMapperPort))
            {
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
                        Port = port,
                        Program = program,
                        Protocol = protocol,
                        Version = version
                    });
            }
        }
    }
}
