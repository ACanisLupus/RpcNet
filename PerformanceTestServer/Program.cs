// Copyright by Artur Wolf

using System.Net;
using RpcNet;
using TestService;

const int Port = 2223;

using var testServer = new TestServer(IPAddress.Any, Port);
testServer.Start();

long lastValue = -1;

while (true)
{
    if (TestServer.Counter != lastValue)
    {
        Console.WriteLine($"Received {TestServer.Counter} calls");
        lastValue = TestServer.Counter;
    }

    Thread.Sleep(1000);
}

internal class TestServer : TestServiceServerStub
{
    public static long Counter;

    public TestServer(IPAddress ipAddress, int port) : base(Protocol.TcpAndUdp, ipAddress, port)
    {
    }

    public override void ThrowsException_1(Caller caller)
    {
    }

    public override int Echo_1(Caller caller, int value)
    {
        _ = Interlocked.Increment(ref Counter);
        return value;
    }

    public override SimpleStruct SimpleStructSimpleStruct_2(Caller caller, SimpleStruct value) => value;
}
