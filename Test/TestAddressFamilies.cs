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
    public void AllCombinations(
        [Values("127.0.0.1", "0.0.0.0", "::1", "::")] string portMapperAddress,
        [Values("127.0.0.1", "0.0.0.0", "::1", "::")] string serverAddress,
        [Values("127.0.0.1", "::1")] string clientAddress,
        [Values(Protocol.Tcp, Protocol.Udp)] Protocol protocol)
    {
        var portMapperIpAddress = IPAddress.Parse(portMapperAddress);
        var serverIpAddress = IPAddress.Parse(serverAddress);
        var clientIpAddress = IPAddress.Parse(clientAddress);

        FilterNotWorkingTests(portMapperIpAddress, serverIpAddress, clientIpAddress, protocol);

        SetUp(portMapperIpAddress, serverIpAddress);

        var clientSettings = new ClientSettings
        {
            PortMapperPort = _portMapperServer.TcpPort
            //Logger = new TestLogger("Test Client")
        };

        using var client = new TestServiceClient(protocol, clientIpAddress, 0, clientSettings);

        int result = client.Echo_1(42);
        Assert.That(result, Is.EqualTo(42));
    }

    private static void FilterNotWorkingTests(IPAddress portMapperIpAddress, IPAddress serverIpAddress, IPAddress clientIpAddress, Protocol protocol)
    {
        if (IsIpv4(serverIpAddress) && IsIpv6(clientIpAddress) && IsUdp(protocol))
        {
            // The same issue occurs for TCP sockets, however, internally the client tries again with the IPv4 address.
            // Since there is no connect for UDP, we would have to run into the timeout. That would take too long
            Assert.Ignore("No retry with IPv4 implemented.");
        }

        if (IsIpv6(serverIpAddress) && IPAddress.IsLoopback(serverIpAddress) && IsIpv4(clientIpAddress) && IsUdp(protocol))
        {
            // The same issue occurs for TCP sockets, however, internally the client tries again with the IPv6 address.
            // Since there is no connect for UDP, we would have to run into the timeout. That would take too long
            Assert.Ignore("Dual stack is not working if the server is listening on loopback only.");
        }

        bool allAddressFamiliesEqual = AllEqual(portMapperIpAddress.AddressFamily, serverIpAddress.AddressFamily, clientIpAddress.AddressFamily);
        if (OperatingSystem.IsWindows() && !allAddressFamiliesEqual)
        {
            if (!IPAddress.IsLoopback(portMapperIpAddress) && IsIpv6(portMapperIpAddress) && !IPAddress.IsLoopback(serverIpAddress) && IsIpv6(serverIpAddress))
            {
                // Dual stack. Should be fast enough
                return;
            }

            Assert.Ignore("This test takes too long on Windows.");
        }
    }

    private static bool AllEqual(AddressFamily family1, AddressFamily family2, AddressFamily family3) => (family1 == family2) && (family1 == family3);
    private static bool IsIpv6(IPAddress ipAddress) => ipAddress.AddressFamily == AddressFamily.InterNetworkV6;
    private static bool IsIpv4(IPAddress ipAddress) => ipAddress.AddressFamily == AddressFamily.InterNetwork;
    private static bool IsUdp(Protocol protocol) => protocol == Protocol.Udp;

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
            PortMapperPort = _portMapperServer.TcpPort
            //Logger = new TestLogger("Test Server")
        };

        _testServer = new TestServer(Protocol.TcpAndUdp, serverIpAddress, 0, serverSettings);
        _testServer.Start();
    }
}
