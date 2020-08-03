namespace RpcNet.Test
{
    using System.Net;
    using TestService;

    internal class TestServer : TestServiceServerStub
    {
        public TestServer(Protocol protocol, IPAddress ipAddress, ServerSettings serverSettings = null) : base(
            protocol,
            ipAddress,
            serverSettings)
        {
        }

        public override PingStruct Ping_1(Caller caller, PingStruct arg1) => arg1;
        public override MyStruct TestMyStruct_1(Caller caller, MyStruct arg1) => arg1;
        public override PingStruct Ping2_2(Caller caller, PingStruct arg1) => arg1;
        public override MyStruct TestMyStruct2_2(Caller caller, MyStruct arg1) => arg1;
    }
}
