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

            public TestServer(IPAddress ipAddress) : base(Protocol.TcpAndUdp, ipAddress, 0, TheLogger)
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
