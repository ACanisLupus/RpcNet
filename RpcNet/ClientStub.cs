namespace RpcNet
{
    using System;
    using System.Net;
    using RpcNet.Internal;

    public abstract class ClientStub
    {
        private readonly INetworkClient networkClient;

        protected ClientStub(Protocol protocol, IPAddress ipAddress, int port, int program)
        {
            switch (protocol)
            {
                case Protocol.Tcp:
                    // TODO
                    break;
                case Protocol.Udp:
                    this.networkClient = new RpcUdpClient(ipAddress, port, (uint)program);
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

        public void Call(int procedure, int version, IXdrWritable argument, IXdrReadable result) =>
            this.networkClient.Call((uint)procedure, (uint)version, argument, result);
    }
}
