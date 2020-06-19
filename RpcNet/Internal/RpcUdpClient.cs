namespace RpcNet.Internal
{
    using System.Net;
    using System.Net.Sockets;

    public class RpcUdpClient : INetworkClient
    {
        private readonly IPEndPoint remoteIpEndPoint;
        private readonly UdpClient client;
        private readonly UdpReader reader;
        private readonly UdpWriter writer;
        private readonly Call call;

        public RpcUdpClient(IPAddress ipAddress, int port, uint program)
        {
            this.remoteIpEndPoint = new IPEndPoint(ipAddress, port);
            this.client = new UdpClient();
            this.reader = new UdpReader(this.client.Client);
            this.writer = new UdpWriter(this.client.Client);
            this.call = new Call(program, this.reader, this.writer);
        }

        public int TimeoutInMilliseconds
        {
            get => this.client.Client.ReceiveTimeout;
            set => this.client.Client.ReceiveTimeout = value;
        }

        public void Call(uint procedure, uint version, IXdrWritable argument, IXdrReadable result)
        {
            this.writer.BeginWriting();
            this.call.SendCall(procedure, version, argument);
            NetworkResult udpResult = this.writer.EndWritingSync(this.remoteIpEndPoint);
            if (udpResult.SocketError != SocketError.Success)
            {
                throw new RpcException($"Could not send UDP message to {this.remoteIpEndPoint}. Socket error: {udpResult.SocketError}.");
            }

            udpResult = this.reader.BeginReadingSync();
            if (udpResult.SocketError != SocketError.Success)
            {
                throw new RpcException($"Could not receive UDP reply from {this.remoteIpEndPoint}. Socket error: {udpResult.SocketError}.");
            }

            this.call.ReceiveResult(result);
            this.reader.EndReading();
        }
    }
}
