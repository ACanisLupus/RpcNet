// Copyright by Artur Wolf

using System.Net;
using RpcNet;
using TestService;

const int Port = 2223;

CancellationToken ct = CancellationToken.None;

await using TestServer testServer = new(IPAddress.Any, Port);
await testServer.StartAsync(ct).ConfigureAwait(false);

long lastValue = -1;

while (true)
{
    if (TestServer.Counter != lastValue)
    {
        Console.WriteLine($"Received {TestServer.Counter} calls");
        lastValue = TestServer.Counter;
    }

    await Task.Delay(1000).ConfigureAwait(false);
}

internal class TestServer(IPAddress ipAddress, int port) : TestServiceServerStub(
    Protocol.Tcp | Protocol.Udp,
    ipAddress,
    port,
    new ServerSettings
    {
        PortMapperPort = 0
    })
{
    public static long Counter;

    public override ValueTask ThrowsException_1Async(RpcEndPoint rpcEndPoint, CancellationToken cancellationToken) => ValueTask.CompletedTask;

    public override ValueTask<int> Echo_1Async(RpcEndPoint rpcEndPoint, int value, CancellationToken cancellationToken)
    {
        _ = Interlocked.Increment(ref Counter);
        return new ValueTask<int>(value);
    }

    public override ValueTask<SimpleStruct> SimpleStructSimpleStruct_2Async(RpcEndPoint rpcEndPoint, SimpleStruct value, CancellationToken cancellationToken) =>
        new(value);
}
