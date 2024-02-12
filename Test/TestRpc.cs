// Copyright by Artur Wolf

namespace Test;

using System.Net;
using NUnit.Framework;
using RpcNet;
using RpcNet.PortMapper;
using TestService;

[TestFixture]
internal sealed class TestRpc
{
    private readonly IPAddress _ipAddress = IPAddress.Loopback;

    private PortMapperServer _portMapperServer;
    private TestServer _testServer;

    [SetUp]
    public void SetUp()
    {
        var portMapperSettings = new ServerSettings
        {
            //Logger = new TestLogger("Port Mapper")
        };

        _portMapperServer = new PortMapperServer(Protocol.Tcp, _ipAddress, 0, portMapperSettings);
        _portMapperServer.Start();

        var serverSettings = new ServerSettings
        {
            PortMapperPort = _portMapperServer.TcpPort
        };

        _testServer = new TestServer(Protocol.TcpAndUdp, _ipAddress, 0, serverSettings);
        _testServer.Start();
    }

    [TearDown]
    public void TearDown()
    {
        _testServer?.Dispose();
        _portMapperServer?.Dispose();
    }

    [Test]
    [TestCase(Protocol.Tcp)]
    [TestCase(Protocol.Udp)]
    public void OneClient(Protocol protocol)
    {
        var clientSettings = new ClientSettings
        {
            PortMapperPort = _portMapperServer.TcpPort
        };
        using var client = new TestServiceClient(protocol, _ipAddress, 0, clientSettings);
        SimpleStruct result = client.SimpleStructSimpleStruct_2(
            new SimpleStruct
            {
                Value = 42
            });
        Assert.That(result.Value, Is.EqualTo(42));
    }
}
