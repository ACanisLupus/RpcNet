namespace RpcNet.Test
{
    using System.Net;
    using TestService;

    internal class TestServer : TestServiceServerStub
    {
        public TestServer(IPAddress ipAddress, int port = 0, ILogger logger = null) : base(ipAddress, port, logger)
        {
        }

        public override PingStruct Ping_1(IPEndPoint remoteIpEndPoint, PingStruct arg1) => arg1;
        public override MyStruct TestMyStruct_1(IPEndPoint remoteIpEndPoint, MyStruct arg1) => arg1;
    }
}
