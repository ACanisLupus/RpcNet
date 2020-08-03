namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using RpcNet.PortMapper;

    // Public for tests
    public class RpcTcpClient : INetworkClient
    {
        private readonly RpcCall call;
        private readonly IPEndPoint remoteIpEndPoint;
        private readonly TcpReader tcpReader;
        private readonly TcpWriter tcpWriter;

        private Socket client;

        public RpcTcpClient(
            IPAddress ipAddress,
            int program,
            int version,
            ClientSettings clientSettings = default)
        {
            int port = clientSettings?.Port ?? 0;
            if (port == 0)
            {
                var portMapperClientSettings = new PortMapperClientSettings
                {
                    Port = clientSettings?.PortMapperPort ?? PortMapperConstants.PortMapperPort
                };
                port = PortMapperUtilities.GetPort(Protocol.Tcp, ipAddress, program, version, portMapperClientSettings);
            }

            this.remoteIpEndPoint = new IPEndPoint(ipAddress, port);
            this.client = new Socket(SocketType.Stream, ProtocolType.Tcp);
            this.ReceiveTimeout = clientSettings?.ReceiveTimeout ?? Utilities.DefaultClientReceiveTimeout;
            this.SendTimeout = clientSettings?.SendTimeout ?? Utilities.DefaultClientSendTimeout;
            this.EstablishConnection();
            this.tcpReader = new TcpReader(this.client);
            this.tcpWriter = new TcpWriter(this.client);
            this.call = new RpcCall(
                program,
                this.remoteIpEndPoint,
                this.tcpReader,
                this.tcpWriter,
                this.ReestablishConnection,
                clientSettings?.Logger);
        }

        public TimeSpan ReceiveTimeout
        {
            get => Utilities.GetReceiveTimeout(this.client);
            set => Utilities.SetReceiveTimeout(this.client, value);
        }

        public TimeSpan SendTimeout
        {
            get => Utilities.GetSendTimeout(this.client);
            set => Utilities.SetSendTimeout(this.client, value);
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
            catch (SocketException e)
            {
                throw new RpcException(
                    $"Could not connect to {this.remoteIpEndPoint}. Socket error: {e.SocketErrorCode}.");
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
