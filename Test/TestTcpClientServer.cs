
namespace RpcNet.Test
{
    using System.Net;
    using NUnit.Framework;
    using RpcNet.Internal;

    class TestTcpClientServer
    {
        [Test]
        public void ServerIsNotRunning()
        {
            IPAddress ipAddress = IPAddress.Loopback;
            int port = 12345;
            int program = 12;

            RpcException exception = Assert.Throws<RpcException>(
                () => _ = new RpcTcpClient(ipAddress, port, program));

            Assert.That(
                exception.Message,
                Is.EqualTo($"Could not connect to {ipAddress}:{port}. Socket error: ConnectionRefused."));
        }
    }
}
