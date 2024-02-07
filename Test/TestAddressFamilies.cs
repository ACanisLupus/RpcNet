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
    [TestCase("127.0.0.1", "127.0.0.1", "127.0.0.1", Protocol.Tcp)]
    [TestCase("0.0.0.0", "127.0.0.1", "127.0.0.1", Protocol.Tcp)]
    [TestCase("127.0.0.1", "0.0.0.0", "127.0.0.1", Protocol.Tcp)]
    [TestCase("0.0.0.0", "0.0.0.0", "127.0.0.1", Protocol.Tcp)]
    [TestCase("::1", "127.0.0.1", "127.0.0.1", Protocol.Tcp)]
    [TestCase("::", "127.0.0.1", "127.0.0.1", Protocol.Tcp)]
    [TestCase("::1", "0.0.0.0", "127.0.0.1", Protocol.Tcp)]
    [TestCase("::", "0.0.0.0", "127.0.0.1", Protocol.Tcp)]
    [TestCase("127.0.0.1", "::1", "127.0.0.1", Protocol.Tcp)]
    [TestCase("0.0.0.0", "::1", "127.0.0.1", Protocol.Tcp)]
    [TestCase("127.0.0.1", "::", "127.0.0.1", Protocol.Tcp)]
    [TestCase("0.0.0.0", "::", "127.0.0.1", Protocol.Tcp)]
    [TestCase("::1", "::1", "127.0.0.1", Protocol.Tcp)]
    [TestCase("::", "::1", "127.0.0.1", Protocol.Tcp)]
    [TestCase("::1", "::", "127.0.0.1", Protocol.Tcp)]
    [TestCase("::", "::", "127.0.0.1", Protocol.Tcp)]
    [TestCase("127.0.0.1", "127.0.0.1", "::1", Protocol.Tcp)]
    [TestCase("0.0.0.0", "127.0.0.1", "::1", Protocol.Tcp)]
    [TestCase("127.0.0.1", "0.0.0.0", "::1", Protocol.Tcp)]
    [TestCase("0.0.0.0", "0.0.0.0", "::1", Protocol.Tcp)]
    [TestCase("::1", "127.0.0.1", "::1", Protocol.Tcp)]
    [TestCase("::", "127.0.0.1", "::1", Protocol.Tcp)]
    [TestCase("::1", "0.0.0.0", "::1", Protocol.Tcp)]
    [TestCase("::", "0.0.0.0", "::1", Protocol.Tcp)]
    [TestCase("127.0.0.1", "::1", "::1", Protocol.Tcp)]
    [TestCase("0.0.0.0", "::1", "::1", Protocol.Tcp)]
    [TestCase("127.0.0.1", "::", "::1", Protocol.Tcp)]
    [TestCase("0.0.0.0", "::", "::1", Protocol.Tcp)]
    [TestCase("::1", "::1", "::1", Protocol.Tcp)]
    [TestCase("::", "::1", "::1", Protocol.Tcp)]
    [TestCase("::1", "::", "::1", Protocol.Tcp)]
    [TestCase("::", "::", "::1", Protocol.Tcp)]
    [TestCase("127.0.0.1", "127.0.0.1", "127.0.0.1", Protocol.Udp)]
    [TestCase("0.0.0.0", "127.0.0.1", "127.0.0.1", Protocol.Udp)]
    [TestCase("127.0.0.1", "0.0.0.0", "127.0.0.1", Protocol.Udp)]
    [TestCase("0.0.0.0", "0.0.0.0", "127.0.0.1", Protocol.Udp)]
    [TestCase("::1", "127.0.0.1", "127.0.0.1", Protocol.Udp)]
    [TestCase("::", "127.0.0.1", "127.0.0.1", Protocol.Udp)]
    [TestCase("::1", "0.0.0.0", "127.0.0.1", Protocol.Udp)]
    [TestCase("::", "0.0.0.0", "127.0.0.1", Protocol.Udp)]
    [TestCase("127.0.0.1", "::1", "127.0.0.1", Protocol.Udp)]
    [TestCase("0.0.0.0", "::1", "127.0.0.1", Protocol.Udp)]
    [TestCase("127.0.0.1", "::", "127.0.0.1", Protocol.Udp)]
    [TestCase("0.0.0.0", "::", "127.0.0.1", Protocol.Udp)]
    [TestCase("::1", "::1", "127.0.0.1", Protocol.Udp)]
    [TestCase("::", "::1", "127.0.0.1", Protocol.Udp)]
    [TestCase("::1", "::", "127.0.0.1", Protocol.Udp)]
    [TestCase("::", "::", "127.0.0.1", Protocol.Udp)]
    [TestCase("127.0.0.1", "127.0.0.1", "::1", Protocol.Udp)]
    [TestCase("0.0.0.0", "127.0.0.1", "::1", Protocol.Udp)]
    [TestCase("127.0.0.1", "0.0.0.0", "::1", Protocol.Udp)]
    [TestCase("0.0.0.0", "0.0.0.0", "::1", Protocol.Udp)]
    [TestCase("::1", "127.0.0.1", "::1", Protocol.Udp)]
    [TestCase("::", "127.0.0.1", "::1", Protocol.Udp)]
    [TestCase("::1", "0.0.0.0", "::1", Protocol.Udp)]
    [TestCase("::", "0.0.0.0", "::1", Protocol.Udp)]
    [TestCase("127.0.0.1", "::1", "::1", Protocol.Udp)]
    [TestCase("0.0.0.0", "::1", "::1", Protocol.Udp)]
    [TestCase("127.0.0.1", "::", "::1", Protocol.Udp)]
    [TestCase("0.0.0.0", "::", "::1", Protocol.Udp)]
    [TestCase("::1", "::1", "::1", Protocol.Udp)]
    [TestCase("::", "::1", "::1", Protocol.Udp)]
    [TestCase("::1", "::", "::1", Protocol.Udp)]
    [TestCase("::", "::", "::1", Protocol.Udp)]
    public void AllCombinations(string portMapperAddress, string serverAddress, string clientAddress, Protocol protocol)
    {
        var portMapperIpAddress = IPAddress.Parse(portMapperAddress);
        var serverIpAddress = IPAddress.Parse(serverAddress);
        var clientIpAddress = IPAddress.Parse(clientAddress);

        FilterNotWorkingTests(serverIpAddress, clientIpAddress, protocol);

        SetUp(portMapperIpAddress, serverIpAddress);

        var clientSettings = new ClientSettings
        {
            PortMapperPort = _portMapperServer.TcpPort,
            //Logger = new TestLogger("Test Client")
        };

        using var client = new TestServiceClient(protocol, clientIpAddress, 0, clientSettings);

        int result = client.Echo_1(42);
        Assert.That(result, Is.EqualTo(42));
    }

    private static void FilterNotWorkingTests(IPAddress serverIpAddress, IPAddress clientIpAddress, Protocol protocol)
    {
        // Everything is fine for TCP
        if (protocol == Protocol.Tcp)
        {
            return;
        }

        if (IsIpv4(serverIpAddress) && IsIpv6(clientIpAddress))
        {
            // The same issue occurs for TCP sockets, however, internally the client tries again with the IPv4 address.
            // Since there is no connect for UDP, we would have to run into the timeout. That would take too long
            Assert.Ignore("No retry with IPv4 implemented.");
        }

        if (IsIpv6(serverIpAddress) && IPAddress.IsLoopback(serverIpAddress) && IsIpv4(clientIpAddress) && !OperatingSystem.IsWindows())
        {
            // The same issue occurs for TCP sockets, however, internally the client tries again with the IPv6 address.
            // Since there is no connect for UDP, we would have to run into the timeout. That would take too long
            Assert.Ignore("Linux does not support dual stack if the server is listening on loopback only.");
        }
    }

    private static bool IsIpv6(IPAddress ipAddress) => ipAddress.AddressFamily == AddressFamily.InterNetworkV6;
    private static bool IsIpv4(IPAddress ipAddress) => ipAddress.AddressFamily == AddressFamily.InterNetwork;

    private void SetUp(IPAddress portMapperIpAddress, IPAddress serverIpAddress)
    {
        var portMapperSettings = new ServerSettings
        {
            //Logger = new TestLogger("Port Mapper")
        };

        _portMapperServer = new PortMapperServer(Protocol.Tcp, portMapperIpAddress, 0, portMapperSettings);
        _portMapperServer.Start();

        var serverSettings = new ServerSettings
        {
            PortMapperPort = _portMapperServer.TcpPort,
            //Logger = new TestLogger("Test Server")
        };

        _testServer = new TestServer(Protocol.TcpAndUdp, serverIpAddress, 0, serverSettings);
        _testServer.Start();
    }
}
