// Copyright by Artur Wolf

using System.Net;
using RpcNet;
using Test;
using TestService;

if ((args.Length != 1) || !IPEndPoint.TryParse(args[0], out IPEndPoint? ipEndPoint))
{
    ipEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
}

using var testServer = new TestServer(ipEndPoint);
testServer.Start();

Thread.Sleep(-1);

internal class TestServer : TestServiceServerStub
{
    private static readonly ILogger _theLogger = new TestLogger("Test Server");

    public TestServer(IPEndPoint ipEndPoint) : base(
        Protocol.TcpAndUdp,
        ipEndPoint.Address,
        ipEndPoint.Port,
        new ServerSettings
        {
            Logger = _theLogger
        })
    {
    }

    public override void ThrowsException_1(RpcEndPoint rpcEndPoint) => throw new NotImplementedException();
    public override int Echo_1(RpcEndPoint rpcEndPoint, int value) => value;
    public override SimpleStruct SimpleStructSimpleStruct_2(RpcEndPoint rpcEndPoint, SimpleStruct value) => value;
}
