namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using RpcNet.PortMapper;

    // Public for tests
    public class RpcUdpClient : INetworkClient
    {
        private readonly RpcCall call;
        private readonly Socket client;

        public RpcUdpClient(
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
                port = PortMapperUtilities.GetPort(Protocol.Udp, ipAddress, program, version, portMapperClientSettings);
            }

            var remoteIpEndPoint = new IPEndPoint(ipAddress, port);
            this.client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.ReceiveTimeout = clientSettings?.ReceiveTimeout ?? Utilities.DefaultClientReceiveTimeout;
            this.SendTimeout = clientSettings?.SendTimeout ?? Utilities.DefaultClientSendTimeout;
            var reader = new UdpReader(this.client);
            var writer = new UdpWriter(this.client);
            this.call = new RpcCall(program, remoteIpEndPoint, reader, writer, null, clientSettings?.Logger);
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
    }
}
