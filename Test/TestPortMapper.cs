namespace Test
{
    using System.Net;
    using System.Threading;
    using NUnit.Framework;
    using RpcNet;
    using RpcNet.PortMapper;

    [TestFixture]
    internal class TestPortMapper
    {
        private const int PortMapperPort = 12345;

        private PortMapperServer server;

        [SetUp]
        public void SetUp()
        {
            var settings = new PortMapperServerSettings
            {
                Logger = new TestLogger("Port Mapper"), Port = PortMapperPort
            };
            this.server = new PortMapperServer(Protocol.TcpAndUdp, IPAddress.Any, settings);
            this.server.Start();
        }

        [TearDown]
        public void TearDown()
        {
            Interlocked.Exchange(ref this.server, null)?.Dispose();
        }

        [Test]
        [TestCase(4711, 4712, Protocol.Tcp, 4713)]
        [TestCase(4714, 4715, Protocol.Udp, 4716)]
        [TestCase(4717, 4718, Protocol.Tcp, 4719)]
        [TestCase(4720, 4721, Protocol.Udp, 4721)]
        public void TestSetAndGet(int port, int program, Protocol protocol, int version)
        {
            var settings = new PortMapperClientSettings
            {
                Port = PortMapperPort
            };
            using var client = new PortMapperClient(
                Protocol.Tcp,
                IPAddress.Loopback,
                settings);
            client.Set(
                new Mapping
                {
                    Port = port,
                    Program = program,
                    Protocol = protocol,
                    Version = version
                });

            int receivedPort = client.GetPort(
                new Mapping
                {
                    Protocol = protocol,
                    Program = program,
                    Version = version
                });

            Assert.That(receivedPort, Is.EqualTo(port));
        }

        [Test]
        [TestCase(1, 2, 3, 2, 42)]
        [TestCase(1, 2, 3, 42, 3)]
        public void TestSetAndWrongGet(int port, int program, int version, int program2, int version2)
        {
            var settings = new PortMapperClientSettings
            {
                Port = PortMapperPort
            };
            using var client = new PortMapperClient(
                Protocol.Tcp,
                IPAddress.Loopback,
                settings);
            client.Set(
                new Mapping
                {
                    Port = port,
                    Program = program,
                    Version = version
                });

            int receivedPort = client.GetPort(
                new Mapping
                {
                    Program = program2,
                    Version = version2
                });

            Assert.That(receivedPort, Is.EqualTo(0));
        }
    }
}
