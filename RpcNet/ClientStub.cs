namespace RpcNet
{
    using System;
    using System.Net;
    using RpcNet.Internal;

    public abstract class ClientStub : IDisposable
    {
        private readonly INetworkClient networkClient;

        protected ClientStub(Protocol protocol, IPAddress ipAddress, int port, int program, int version, ILogger logger)
        {
            switch (protocol)
            {
                case Protocol.Tcp:
                    this.networkClient = new RpcTcpClient(ipAddress, port, program, version, logger);
                    break;
                case Protocol.Udp:
                    this.networkClient = new RpcUdpClient(ipAddress, port, program, version, logger);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(protocol));
            }
        }

        public int TimeoutInMilliseconds
        {
            get => this.networkClient.TimeoutInMilliseconds;
            set => this.networkClient.TimeoutInMilliseconds = value;
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
