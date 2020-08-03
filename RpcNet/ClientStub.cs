namespace RpcNet
{
    using System;
    using System.Net;
    using RpcNet.Internal;

    public abstract class ClientStub : IDisposable
    {
        private readonly INetworkClient networkClient;

        protected ClientStub(
            Protocol protocol,
            IPAddress ipAddress,
            int program,
            int version,
            ClientSettings clientSettings = default)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            switch (protocol)
            {
                case Protocol.Tcp:
                    this.networkClient = new RpcTcpClient(ipAddress, program, version, clientSettings);
                    break;
                case Protocol.Udp:
                    this.networkClient = new RpcUdpClient(ipAddress, program, version, clientSettings);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(protocol));
            }
        }

        public TimeSpan ReceiveTimeout
        {
            get => this.networkClient.ReceiveTimeout;
            set => this.networkClient.ReceiveTimeout = value;
        }

        public TimeSpan SendTimeout
        {
            get => this.networkClient.SendTimeout;
            set => this.networkClient.SendTimeout = value;
        }

        public void Call(int procedure, int version, IXdrWritable argument, IXdrReadable result)
        {
            this.networkClient.Call(procedure, version, argument, result);
        }

        public void Dispose()
        {
            this.networkClient.Dispose();
        }
    }
}
