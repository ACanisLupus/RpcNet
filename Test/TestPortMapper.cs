namespace RpcNet.Test
{
    using System.Net;
    using System.Threading;
    using NUnit.Framework;

    internal class TestPortMapper
    {
        private const int Port = 12345;

        private PortMapperServer server;

        [SetUp]
        public void SetUp()
        {
            this.server = new PortMapperServer(IPAddress.Any, Port);
            this.server.Start();
        }

        [TearDown]
        public void TearDown()
        {
            Interlocked.Exchange(ref this.server, null)?.Dispose();
        }

        [Test]
        [TestCase(4711, 4712, ProtocolKind.Tcp, 4713)]
        [TestCase(4714, 4715, ProtocolKind.Udp, 4716)]
        [TestCase(4717, 4718, ProtocolKind.Tcp, 4719)]
        [TestCase(4720, 4721, ProtocolKind.Udp, 4721)]
        public void TestSetAndGet(int port, int program, ProtocolKind protocol, int version)
        {
            using var client = new PortMapperClient(
                Protocol.Tcp,
                IPAddress.Loopback,
                Port);
            client.Set_2(
                new Mapping
                {
                    Port = port,
                    Program = program,
                    Protocol = protocol,
                    Version = version
                });

            int receivedPort = client.GetPort_2(
                new Mapping
                {
                    Protocol = protocol,
                    Program = program,
                    Version = version
                });

            Assert.That(receivedPort, Is.EqualTo(port));
        }

        [Test]
        [TestCase(1, 2, ProtocolKind.Tcp, 3, 2, ProtocolKind.Tcp, 42)]
        [TestCase(1, 2, ProtocolKind.Udp, 3, 2, ProtocolKind.Tcp, 3)]
        [TestCase(1, 2, ProtocolKind.Udp, 3, 42, ProtocolKind.Tcp, 3)]
        public void TestSetAndWrongGet(
            int port,
            int program,
            ProtocolKind protocol,
            int version,
            int program2,
            ProtocolKind protocol2,
            int version2)
        {
            using var client = new PortMapperClient(
                Protocol.Tcp,
                IPAddress.Loopback,
                Port);
            client.Set_2(
                new Mapping
                {
                    Port = port,
                    Program = program,
                    Protocol = protocol,
                    Version = version
                });

            int receivedPort = client.GetPort_2(
                new Mapping
                {
                    Protocol = protocol2,
                    Program = program2,
                    Version = version2
                });

            Assert.That(receivedPort, Is.EqualTo(0));
        }
    }
}
