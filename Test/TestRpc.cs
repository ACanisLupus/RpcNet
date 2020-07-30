namespace RpcNet.Test
{
    using System.Net;
    using NUnit.Framework;
    using TestService;

    internal class TestRpc
    {
        private const int Port = 12345;

        private TestServer testServer;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }

        [SetUp]
        public void SetUp()
        {
            this.testServer = new TestServer(
                Protocol.TcpAndUdp,
                IPAddress.Loopback,
                Port,
                new TestLogger("RPC Server"));
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
            using var client = new TestServiceClient(protocol, IPAddress.Loopback, Port, new TestLogger("RPC Client"));
            for (int i = 0; i < 100; i++)
            {
                PingStruct result = client.Ping_1(new PingStruct { Value = i });
                Assert.That(result.Value, Is.EqualTo(i));
            }
        }
    }
}
