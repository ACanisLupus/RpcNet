namespace TestServer
{
    using System;
    using System.Net;
    using System.Threading;
    using RpcNet;
    using TestService;

    class Program
    {
        static void Main()
        {
            using (var testServer = new TestServer(IPAddress.Any))
            {
                Thread.Sleep(-1);
            }
        }

        class TestServer : TestServiceServerStub
        {
            private static readonly ILogger logger = new Logger();

            public TestServer(IPAddress ipAddress) : base(ipAddress, 0, logger)
            {
            }

            public override PingStruct Ping_1(IPEndPoint remoteIpEndPoint, PingStruct arg1)
            {
                logger.Info($"{remoteIpEndPoint} PING({arg1.Value})");
                return arg1;
            }

            public override MyStruct TestMyStruct_1(IPEndPoint remoteIpEndPoint, MyStruct arg1)
            {
                logger.Info($"{remoteIpEndPoint} TESTMYSTRUCT");
                return arg1;
            }
        }

        class Logger : ILogger
        {
            public void Error(string entry) => Console.WriteLine("ERROR " + entry);
            public void Info(string entry) => Console.WriteLine("INFO  " + entry);
            public void Trace(string entry) => Console.WriteLine("TRACE " + entry);
        }
    }
}
