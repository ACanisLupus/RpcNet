namespace Test
{
    using System.Net;
    using NUnit.Framework;
    using RpcNet;
    using RpcNet.PortMapper;
    using TestService;

    [TestFixture]
    internal class TestRpc
    {
        private const int PortMapperPort = 11111;

        private PortMapperServer portMapperServer;
        private TestServer testServer;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var settings = new PortMapperServerSettings
            {
                Logger = new TestLogger("Port Mapper"),
                Port = PortMapperPort
            };

            this.portMapperServer = new PortMapperServer(Protocol.TcpAndUdp, IPAddress.Loopback, settings);
            this.portMapperServer.Start();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            this.portMapperServer?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            var serverSettings = new ServerSettings
            {
                Logger = new TestLogger("RPC Server"),
                PortMapperPort = PortMapperPort
            };

            this.testServer = new TestServer(Protocol.TcpAndUdp, IPAddress.Loopback, serverSettings);
            this.testServer.Start();
        }

        [TearDown]
        public void TearDown()
        {
            this.testServer?.Dispose();
        }

        [Test]
        [TestCase(Protocol.Tcp)]
        [TestCase(Protocol.Udp)]
        public void OneClient(Protocol protocol)
        {
            var clientSettings = new ClientSettings
            {
                Logger = new TestLogger("RPC Client"),
                PortMapperPort = PortMapperPort
            };
            using var client = new TestServiceClient(protocol, IPAddress.Loopback, clientSettings);
            for (int i = 0; i < 100; i++)
            {
                PingStruct result = client.Ping_1(new PingStruct { Value = i });
                Assert.That(result.Value, Is.EqualTo(i));
            }
        }
    }
}
