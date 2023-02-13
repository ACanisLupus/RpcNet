// Copyright by Artur Wolf

namespace Test;

using System.Net;
using RpcNet;
using TestService;

internal class TestServer : TestServiceServerStub
{
    public TestServer(Protocol protocol, IPAddress ipAddress, int port, ServerSettings serverSettings = null) : base(
        protocol,
        ipAddress,
        port,
        serverSettings)
    {
    }

    public override void VoidVoid1_1(Caller caller) => throw new NotImplementedException();

    public override void VoidVoid2_1(Caller caller) => throw new NotImplementedException();

    public override int IntInt1_1(Caller caller, int value) => value;

    public override int IntInt2_1(Caller caller, int int32) => throw new NotImplementedException();

    public override SimpleStruct SimpleStructSimpleStruct_2(Caller caller, SimpleStruct value) => value;
}
