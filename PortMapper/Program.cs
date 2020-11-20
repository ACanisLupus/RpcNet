namespace PortMapper
{
    using System.Net;
    using System.Threading;
    using RpcNet;
    using RpcNet.PortMapper;
    using RpcNet.Test;

    internal class Program
    {
        private static void Main()
        {
            using var portMapperServer = new PortMapperServer(
                Protocol.TcpAndUdp,
                IPAddress.Any,
                new PortMapperServerSettings { Logger = new TestLogger("Port Mapper") });
            portMapperServer.Start();

            Thread.Sleep(-1);
        }
    }
}
