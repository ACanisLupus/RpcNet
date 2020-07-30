namespace PortMapper
{
    using System.Net;
    using System.Threading;
    using RpcNet;
    using RpcNet.Test;

    internal class Program
    {
        private static void Main()
        {
            using var portMapperServer = new PortMapperServer(IPAddress.Any, null, new TestLogger("Port Mapper"));
            portMapperServer.Start();

            Thread.Sleep(-1);
        }
    }
}
