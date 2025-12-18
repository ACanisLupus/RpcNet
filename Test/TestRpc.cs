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

    private PortMapperServer _portMapperServer = null!;
    private TestServer _testServer = null!;

    [SetUp]
    public void SetUp()
    {
        ServerSettings portMapperSettings = new()
        {
            //Logger = new TestLogger("Port Mapper")
        };

        _portMapperServer = new PortMapperServer(Protocol.Tcp, _ipAddress, 0, portMapperSettings);
        _portMapperServer.Start();

        ServerSettings serverSettings = new()
        {
            PortMapperPort = _portMapperServer.TcpPort
        };

        _testServer = new TestServer(Protocol.Tcp | Protocol.Udp, _ipAddress, 0, serverSettings);
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
        ClientSettings clientSettings = new()
        {
            PortMapperPort = _portMapperServer.TcpPort
        };
        using TestServiceClient client = new(protocol, _ipAddress, 0, clientSettings);
        SimpleStruct result = client.SimpleStructSimpleStruct_2(
            new SimpleStruct
            {
                Value = 42
            });
        Assert.That(result.Value, Is.EqualTo(42));
    }
}
