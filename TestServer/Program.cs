namespace TestServer
{
    using System.Net;
    using System.Threading;
    using RpcNet;
    using RpcNet.Test;
    using TestService;

    internal class Program
    {
        private static void Main()
        {
            using var testServer = new TestServer(IPAddress.Any);
            testServer.Start();

            Thread.Sleep(-1);
        }

        private class TestServer : TestServiceServerStub
        {
            private static readonly ILogger TheLogger = new TestLogger("Test Server");

            public TestServer(IPAddress ipAddress) : base(
                Protocol.TcpAndUdp,
                ipAddress,
                new ServerSettings { Logger = TheLogger })
            {
            }

            public override PingStruct Ping_1(Caller caller, PingStruct arg1)
            {
                TheLogger.Info($"{caller} PING1({arg1.Value})");
                return arg1;
            }

            public override MyStruct TestMyStruct_1(Caller caller, MyStruct arg1)
            {
                TheLogger.Info($"{caller} TESTMYSTRUCT");
                return arg1;
            }

            public override PingStruct Ping2_2(Caller caller, PingStruct arg1)
            {
                TheLogger.Info($"{caller} PING2({arg1.Value})");
                return arg1;
            }

            public override MyStruct TestMyStruct2_2(Caller caller, MyStruct arg1)
            {
                TheLogger.Info($"{caller} TESTMYSTRUCT2");
                return arg1;
            }
        }
    }
}
