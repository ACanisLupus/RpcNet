// Copyright by Artur Wolf

namespace Test;

using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using RpcNet;
using RpcNet.PortMapper;
using TestService;

[TestFixture]
internal sealed class TestAddressFamilies
{
    private PortMapperServer _portMapperServer;
    private TestServer _testServer;

    [TearDown]
    public void TearDown()
    {
        _testServer?.Dispose();
        _portMapperServer?.Dispose();
    }

    [Test]
    [TestCase(AddressFamily.InterNetwork, AddressFamily.InterNetwork, AddressFamily.InterNetwork, Protocol.Tcp)]
    [TestCase(AddressFamily.InterNetwork, AddressFamily.InterNetwork, AddressFamily.InterNetwork, Protocol.Udp)]
    [TestCase(AddressFamily.InterNetworkV6, AddressFamily.InterNetworkV6, AddressFamily.InterNetworkV6, Protocol.Tcp)]
    [TestCase(AddressFamily.InterNetworkV6, AddressFamily.InterNetworkV6, AddressFamily.InterNetworkV6, Protocol.Udp)]
    public void AllCombinations(AddressFamily portMapperAddressFamily, AddressFamily serverAddressFamily, AddressFamily clientAddressFamily, Protocol protocol)
    {
        SetUp(portMapperAddressFamily, serverAddressFamily);

        var clientSettings = new ClientSettings { PortMapperPort = _portMapperServer.TcpPort, Logger = new TestLogger("Test Client") };
        using var client = new TestServiceClient(protocol, GetLoopbackIpAddressForAddressFamily(clientAddressFamily), 0, clientSettings);

        int result = client.Echo_1(42);
        Assert.That(result, Is.EqualTo(42));
    }

    private void SetUp(AddressFamily portMapperAddressFamily, AddressFamily serverAddressFamily)
    {
        var portMapperSettings = new ServerSettings { Logger = new TestLogger("Port Mapper") };

        _portMapperServer = new PortMapperServer(Protocol.Tcp, GetLoopbackIpAddressForAddressFamily(portMapperAddressFamily), 0, portMapperSettings);
        _portMapperServer.Start();

        var serverSettings = new ServerSettings { PortMapperPort = _portMapperServer.TcpPort, Logger = new TestLogger("Test Server") };

        _testServer = new TestServer(Protocol.TcpAndUdp, GetLoopbackIpAddressForAddressFamily(serverAddressFamily), 0, serverSettings);
        _testServer.Start();
    }

    private static IPAddress GetLoopbackIpAddressForAddressFamily(AddressFamily addressFamily)
    {
        if (addressFamily == AddressFamily.InterNetwork)
        {
            return IPAddress.Loopback;
        }

        return IPAddress.IPv6Loopback;
    }
}
