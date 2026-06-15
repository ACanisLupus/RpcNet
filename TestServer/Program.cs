// Copyright by Artur Wolf

using System.Net;
using RpcNet;
using Test;
using TestService;

CancellationToken ct = CancellationToken.None;

if ((args.Length != 1) || !IPEndPoint.TryParse(args[0], out IPEndPoint? ipEndPoint))
{
    ipEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
}

await using TestServer testServer = new(ipEndPoint);
await testServer.StartAsync(ct).ConfigureAwait(false);

Thread.Sleep(-1);

internal class TestServer(IPEndPoint ipEndPoint) : TestServiceServerStub(
    Protocol.Tcp | Protocol.Udp,
    ipEndPoint.Address,
    ipEndPoint.Port,
    new ServerSettings
    {
        Logger = _theLogger
    })
{
    private static readonly ILogger _theLogger = new TestLogger("Test Server");

    public override ValueTask ThrowsException_1Async(RpcEndPoint rpcEndPoint, CancellationToken cancellationToken) => throw new NotImplementedException();
    public override ValueTask<int> Echo_1Async(RpcEndPoint rpcEndPoint, int value, CancellationToken cancellationToken) => new(value);

    public override ValueTask<SimpleStruct> SimpleStructSimpleStruct_2Async(RpcEndPoint rpcEndPoint, SimpleStruct value, CancellationToken cancellationToken) =>
        new(value);
}
