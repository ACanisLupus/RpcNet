namespace RpcNet.Test
{
    using System;
    using System.Net;
    using NUnit.Framework;
    using TestService;

    internal class TestRpc
    {
        private PortMapperServer portMapperServer;
        private TestServer testServer;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            this.portMapperServer = new PortMapperServer(IPAddress.Loopback, new MyLogger("PMAP"));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            this.portMapperServer?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            this.testServer = new TestServer(IPAddress.Loopback, 0, new MyLogger("TEST"));
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
            using (var client = new TestServiceClient(protocol, IPAddress.Loopback))
            {
                for (int i = 0; i < 100; i++)
                {
                    PingStruct result = client.Ping_1(new PingStruct { Value = i });
                    Assert.That(result.Value, Is.EqualTo(i));
                }
            }
        }

        private class MyLogger : ILogger
        {
            private readonly string name;

            public MyLogger(string name)
            {
                this.name = name;
            }

            /// <inheritdoc />
            public void Trace(string entry) => Console.WriteLine(this.name + " TRACE " + entry);

            /// <inheritdoc />
            public void Info(string entry) => Console.WriteLine(this.name + " INFO  " + entry);

            /// <inheritdoc />
            public void Error(string entry) => Console.WriteLine(this.name + " ERROR " + entry);
        }
    }
}
