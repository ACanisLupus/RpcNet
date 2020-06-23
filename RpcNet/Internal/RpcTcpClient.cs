namespace RpcNet.Internal
{
    using System.Net;
    using System.Net.Sockets;

    public class RpcTcpClient : INetworkClient
    {
        private readonly IPEndPoint remoteIpEndPoint;
        private readonly TcpReader reader;
        private readonly TcpWriter writer;
        private readonly Call call;

        private TcpClient client;

        public RpcTcpClient(IPAddress ipAddress, int port, int program)
        {
            this.remoteIpEndPoint = new IPEndPoint(ipAddress, port);
            this.client = new TcpClient();
            this.EstablishConnection();
            this.reader = new TcpReader(this.client.Client);
            this.writer = new TcpWriter(this.client.Client);
            this.call = new Call(
                program,
                this.remoteIpEndPoint,
                this.reader,
                this.writer,
                this.ReestablishConnection);
            this.TimeoutInMilliseconds = 10000;
        }

        public int TimeoutInMilliseconds
        {
            get => this.client.Client.ReceiveTimeout;
            set => this.client.Client.ReceiveTimeout = value;
        }

        public void Call(int procedure, int version, IXdrWritable argument, IXdrReadable result)
            => this.call.SendCall(procedure, version, argument, result);

        public void Dispose()
        {
            this.client.Dispose();
            //this.reader.Dispose();
            //this.writer.Dispose();
        }

        private void EstablishConnection()
        {
            try
            {
                this.client.Connect(this.remoteIpEndPoint);
            }
            catch (SocketException exception)
            {
                throw new RpcException(
                    $"Could not connect to {this.remoteIpEndPoint}. Socket error: {exception.SocketErrorCode}.");
            }
        }

        private void ReestablishConnection()
        {
            this.client.Close();
            this.client = new TcpClient();
            this.EstablishConnection();
        }
    }
}
