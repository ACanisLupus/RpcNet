// Copyright by Artur Wolf

namespace PerformanceTestServer;

using System.Net;
using RpcNet;
using TestService;

internal class Program
{
    private const int Port = 2223;
    private static long _counter;

    private static void Main()
    {
        using var testServer = new TestServer(IPAddress.Any, Port);
        testServer.Start();

        long lastValue = -1;
        while (true)
        {
            if (_counter != lastValue)
            {
                Console.WriteLine($"Received {_counter} calls");
                lastValue = _counter;
            }

            Thread.Sleep(1000);
        }

        // ReSharper disable once FunctionNeverReturns
    }

    private class TestServer : TestServiceServerStub
    {
        public TestServer(IPAddress ipAddress, int port) : base(Protocol.TcpAndUdp, ipAddress, port)
        {
        }

        public override void VoidVoid1_1(Caller caller)
        {
        }

        public override void VoidVoid2_1(Caller caller)
        {
        }

        public override int IntInt1_1(Caller caller, int value)
        {
            _ = Interlocked.Increment(ref _counter);
            return value;
        }

        public override int IntInt2_1(Caller caller, int int32) => int32;
        public override SimpleStruct SimpleStructSimpleStruct_2(Caller caller, SimpleStruct value) => value;
    }
}
