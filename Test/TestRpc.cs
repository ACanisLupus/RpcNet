// Copyright by Artur Wolf

namespace Test;

using System.Net;
using NUnit.Framework;
using RpcNet;
using TestService;

[TestFixture]
internal class TestRpc : TestBase
{
    private TestServer _testServer;

    [SetUp]
    public void SetUp()
    {
        var serverSettings = new ServerSettings { PortMapperPort = PortMapperPort };

        _testServer = new TestServer(Protocol.TcpAndUdp, IPAddress.Loopback, 0, serverSettings);
        _testServer.Start();
    }

    [TearDown]
    public void TearDown() => _testServer?.Dispose();

    [Test]
    [TestCase(Protocol.Tcp)]
    [TestCase(Protocol.Udp)]
    public void OneClient(Protocol protocol)
    {
        var clientSettings = new ClientSettings { PortMapperPort = PortMapperPort };
        using var client = new TestServiceClient(protocol, IPAddress.Loopback, 0, clientSettings);
        for (int i = 0; i < 100; i++)
        {
            SimpleStruct result = client.SimpleStructSimpleStruct_2(new SimpleStruct { Value = i });
            Assert.That(result.Value, Is.EqualTo(i));
        }
    }
}
