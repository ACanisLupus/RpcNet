namespace RpcNet.Internal
{
    using System.Net;
    using System.Net.Sockets;

    public class RpcUdpClient : INetworkClient
    {
        private readonly Call call;
        private readonly UdpClient client;
        private readonly UdpReader reader;
        private readonly UdpWriter writer;

        public RpcUdpClient(IPAddress ipAddress, int port, int program, int version, ILogger logger)
        {
            if (port == 0)
            {
                port = PortMapperUtilities.GetPort(ProtocolKind.Udp, ipAddress, program, version);
            }

            var remoteIpEndPoint = new IPEndPoint(ipAddress, port);
            this.client = new UdpClient();
            this.reader = new UdpReader(this.client, logger);
            this.writer = new UdpWriter(this.client, remoteIpEndPoint, logger);
            this.call = new Call(program, remoteIpEndPoint, this.reader, this.writer, null, logger);
            this.TimeoutInMilliseconds = 10000;
        }

        public int TimeoutInMilliseconds
        {
            get => this.client.Client.ReceiveTimeout;
            set => this.client.Client.ReceiveTimeout = value;
        }

        public void Call(int procedure, int version, IXdrWritable argument, IXdrReadable result)
        {
            this.call.SendCall(procedure, version, argument, result);
        }

        public void Dispose()
        {
            this.client.Dispose();
            this.reader.Dispose();
            this.writer.Dispose();
        }
    }
}
