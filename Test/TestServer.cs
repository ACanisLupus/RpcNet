// Copyright by Artur Wolf

namespace Test;

using System.Net;
using RpcNet;
using TestService;

internal sealed class TestServer : TestServiceServerStub
{
    public TestServer(Protocol protocol, IPAddress ipAddress, int port, ServerSettings serverSettings = null) : base(protocol, ipAddress, port, serverSettings)
    {
    }

    public override void ThrowsException_1(Caller caller) => throw new NotSupportedException();
    public override int Echo_1(Caller caller, int value) => value;
    public override SimpleStruct SimpleStructSimpleStruct_2(Caller caller, SimpleStruct value) => value;
}
