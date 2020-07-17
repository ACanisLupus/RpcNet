namespace RpcNet.Test
{
    using System.Net;
    using TestService;

    internal class TestServer : TestServiceServerStub
    {
        public TestServer(Protocols protocols, IPAddress ipAddress, int port = 0, ILogger logger = null) : base(
            protocols,
            ipAddress,
            port,
            logger)
        {
        }

        public override PingStruct Ping_1(IPEndPoint remoteIpEndPoint, PingStruct arg1) => arg1;
        public override MyStruct TestMyStruct_1(IPEndPoint remoteIpEndPoint, MyStruct arg1) => arg1;
        public override PingStruct Ping2_2(IPEndPoint remoteIpEndPoint, PingStruct arg1) => arg1;
        public override MyStruct TestMyStruct2_2(IPEndPoint remoteIpEndPoint, MyStruct arg1) => arg1;
    }
}
