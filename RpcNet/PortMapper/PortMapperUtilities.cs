namespace RpcNet.PortMapper
{
    using System.Net;

    internal static class PortMapperUtilities
    {
        public static int GetPort(
            Protocol protocol,
            IPAddress ipAddress,
            int program,
            int version,
            PortMapperClientSettings clientSettings = default)
        {
            using (var portMapperClient = new PortMapperClient(Protocol.Tcp, ipAddress, clientSettings))
            {
                return portMapperClient.GetPort(
                    new Mapping
                    {
                        Program = program,
                        Protocol = protocol,
                        Version = version
                    });
            }
        }

        public static void UnsetAndSetPort(
            Protocol protocol,
            int port,
            int program,
            int version,
            PortMapperClientSettings clientSettings = default)
        {
            using (var portMapperClient = new PortMapperClient(Protocol.Tcp, IPAddress.Loopback, clientSettings))
            {
                portMapperClient.Unset(
                    new Mapping
                    {
                        Program = program,
                        Protocol = protocol,
                        Version = version
                    });
                portMapperClient.Set(
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
