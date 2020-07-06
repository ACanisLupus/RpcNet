namespace RpcNet.Internal
{
    using System.Net;
    using System.Net.Sockets;

    public class RpcTcpClient : INetworkClient
    {
        private readonly ILogger logger;
        private readonly Call call;
        private readonly IPEndPoint remoteIpEndPoint;

        private TcpClient client;

        public RpcTcpClient(IPAddress ipAddress, int port, int program, int version, ILogger logger)
        {
            if (port == 0)
            {
                port = PortMapperUtilities.GetPort(ProtocolKind.Tcp, ipAddress, program, version);
            }

            this.logger = logger;
            this.remoteIpEndPoint = new IPEndPoint(ipAddress, port);
            this.client = new TcpClient();
            this.EstablishConnection();
            var reader = new TcpReader(this.client.Client, logger);
            var writer = new TcpWriter(this.client.Client);
            this.call = new Call(program, this.remoteIpEndPoint, reader, writer, this.ReestablishConnection, logger);
            this.TimeoutInMilliseconds = 10000;
        }

        public int TimeoutInMilliseconds
        {
            get => this.client.Client.ReceiveTimeout;
            set => this.client.Client.ReceiveTimeout = value;
        }

        public void Call(int procedure, int version, IXdrWritable argument, IXdrReadable result) =>
            this.call.SendCall(procedure, version, argument, result);

        public void Dispose() => this.client.Dispose();

        private void EstablishConnection()
        {
            try
            {
                this.client.Connect(this.remoteIpEndPoint);
            }
            catch (SocketException exception)
            {
                string errorMessage =
                    $"Could not connect to {this.remoteIpEndPoint}. Socket error: {exception.SocketErrorCode}.";
                this.logger?.Error(errorMessage);
                throw new RpcException(errorMessage);
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
