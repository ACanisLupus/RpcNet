namespace TestServer
{
    using System;
    using System.Net;
    using System.Threading;
    using RpcNet;
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
            private static readonly ILogger TheLogger = new Logger();

            public TestServer(IPAddress ipAddress) : base(Protocols.TcpAndUdp, ipAddress, 0, TheLogger)
            {
            }

            public override PingStruct Ping_1(IPEndPoint remoteIpEndPoint, PingStruct arg1)
            {
                TheLogger.Info($"{remoteIpEndPoint} PING1({arg1.Value})");
                return arg1;
            }

            public override MyStruct TestMyStruct_1(IPEndPoint remoteIpEndPoint, MyStruct arg1)
            {
                TheLogger.Info($"{remoteIpEndPoint} TESTMYSTRUCT");
                return arg1;
            }

            public override PingStruct Ping2_2(IPEndPoint remoteIpEndPoint, PingStruct arg1)
            {
                TheLogger.Info($"{remoteIpEndPoint} PING2({arg1.Value})");
                return arg1;
            }

            public override MyStruct TestMyStruct2_2(IPEndPoint remoteIpEndPoint, MyStruct arg1)
            {
                TheLogger.Info($"{remoteIpEndPoint} TESTMYSTRUCT2");
                return arg1;
            }
        }

        private class Logger : ILogger
        {
            public void Error(string entry)
            {
                Console.WriteLine("ERROR " + entry);
            }

            public void Info(string entry)
            {
                Console.WriteLine("INFO  " + entry);
            }

            public void Trace(string entry)
            {
                Console.WriteLine("TRACE " + entry);
            }
        }
    }
}
