namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public class RpcUdpClient : INetworkClient, IDisposable
    {
        private readonly IPEndPoint remoteIpEndPoint;
        private readonly UdpClient client;
        private readonly UdpReader reader;
        private readonly UdpWriter writer;
        private readonly Call call;

        public RpcUdpClient(IPAddress ipAddress, int port, int program)
        {
            this.remoteIpEndPoint = new IPEndPoint(ipAddress, port);
            this.client = new UdpClient();
            this.reader = new UdpReader(this.client.Client);
            this.writer = new UdpWriter(this.client.Client, this.remoteIpEndPoint);
            this.call = new Call(program, this.remoteIpEndPoint, this.reader, this.writer);
        }

        public int TimeoutInMilliseconds
        {
            get => this.client.Client.ReceiveTimeout;
            set => this.client.Client.ReceiveTimeout = value;
        }

        public void Call(int procedure, int version, IXdrWritable argument, IXdrReadable result) => this.call.CallRpc(procedure, version, argument, result);

        public void Dispose()
        {
            this.client.Dispose();
            this.reader.Dispose();
            this.writer.Dispose();
        }
    }
}
