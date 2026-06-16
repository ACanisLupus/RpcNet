// Copyright by Artur Wolf

namespace Test;

using System.Net;
using RpcNet;
using TestService;

internal sealed class TestServer(Protocol protocol, IPAddress ipAddress, int port, ServerSettings serverSettings)
    : TestServiceServerStub(protocol, ipAddress, port, serverSettings)
{
    public override ValueTask ThrowsException_1Async(RpcEndPoint rpcEndPoint, CancellationToken cancellationToken) => throw new NotSupportedException();
    public override ValueTask<int> Echo_1Async(RpcEndPoint rpcEndPoint, int value, CancellationToken cancellationToken) => new(value);

    public override ValueTask<SimpleStruct> SimpleStructSimpleStruct_2Async(RpcEndPoint rpcEndPoint, SimpleStruct value, CancellationToken cancellationToken) =>
        new(value);
}
