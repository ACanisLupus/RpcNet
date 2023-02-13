// Copyright by Artur Wolf

namespace TestServer;

using System.Net;
using RpcNet;
using Test;
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
        private static readonly ILogger _theLogger = new TestLogger("Test Server");

        public TestServer(IPAddress ipAddress) : base(
            Protocol.TcpAndUdp,
            ipAddress,
            0,
            new ServerSettings { Logger = _theLogger })
        {
        }

        public override void VoidVoid1_1(Caller caller)
        {
        }

        public override void VoidVoid2_1(Caller caller)
        {
        }

        public override int IntInt1_1(Caller caller, int value) => value;

        public override int IntInt2_1(Caller caller, int int32) => int32;

        public override SimpleStruct SimpleStructSimpleStruct_2(Caller caller, SimpleStruct value) => value;
    }
}
