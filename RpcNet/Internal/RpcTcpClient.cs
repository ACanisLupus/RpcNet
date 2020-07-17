namespace RpcNet.Internal
{
    using System.Net;
    using System.Net.Sockets;

    public class RpcTcpClient : INetworkClient
    {
        private readonly Call call;
        private readonly IPEndPoint remoteIpEndPoint;
        private readonly TcpReader tcpReader;
        private readonly TcpWriter tcpWriter;

        private Socket client;

        public RpcTcpClient(IPAddress ipAddress, int port, int program, int version, ILogger logger)
        {
            if (port == 0)
            {
                port = PortMapperUtilities.GetPort(ProtocolKind.Tcp, ipAddress, program, version);
            }

            this.remoteIpEndPoint = new IPEndPoint(ipAddress, port);
            this.client = new Socket(SocketType.Stream, ProtocolType.Tcp);
            this.EstablishConnection();
            this.tcpReader = new TcpReader(this.client);
            this.tcpWriter = new TcpWriter(this.client);
            this.call = new Call(
                program,
                this.remoteIpEndPoint,
                this.tcpReader,
                this.tcpWriter,
                this.ReestablishConnection,
                logger);
            this.TimeoutInMilliseconds = 10000;
        }

        public int TimeoutInMilliseconds
        {
            get => this.client.ReceiveTimeout;
            set => this.client.ReceiveTimeout = value;
        }

        public void Call(int procedure, int version, IXdrWritable argument, IXdrReadable result)
        {
            this.call.SendCall(procedure, version, argument, result);
        }

        public void Dispose()
        {
            this.client.Dispose();
        }

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
                throw new RpcException(errorMessage);
            }
        }

        private void ReestablishConnection()
        {
            this.client.Close();
            this.client = new Socket(SocketType.Stream, ProtocolType.Tcp);
            this.EstablishConnection();
            this.tcpReader.Reset(this.client);
            this.tcpWriter.Reset(this.client);
        }
    }
}
