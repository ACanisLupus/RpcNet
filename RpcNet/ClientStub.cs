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
                    port = this.GetPort(PortMapperConstants.ProtocolTcp, ipAddress, port, program, version);
                    this.networkClient = new RpcTcpClient(ipAddress, port, program, logger);
                    break;
                case Protocol.Udp:
                    port = this.GetPort(PortMapperConstants.ProtocolUdp, ipAddress, port, program, version);
                    this.networkClient = new RpcUdpClient(ipAddress, port, program, logger);
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
            this.networkClient.Call(procedure, version, argument, result);

        public void Dispose() => this.networkClient.Dispose();

        private int GetPort(int protocol, IPAddress ipAddress, int port, int program, int version)
        {
            if (port != 0)
            {
                return port;
            }

            using (var portMapperClient = new PortMapperClient(
                Protocol.Tcp,
                ipAddress,
                PortMapperConstants.PortMapperPort))
            {
                return (int)portMapperClient.GetPort_2(
                    new Mapping
                    {
                        Program = (uint)program,
                        Protocol = (uint)protocol,
                        Version = (uint)version
                    });
            }
        }
    }
}
