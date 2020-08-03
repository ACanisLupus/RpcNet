namespace RpcNet
{
    using System;
    using System.Net;
    using RpcNet.Internal;

    public abstract class ServerStub : IDisposable
    {
        private readonly RpcTcpServer rpcTcpServer;
        private readonly RpcUdpServer rpcUdpServer;

        private bool isDisposed;

        protected ServerStub(
            Protocol protocol,
            IPAddress ipAddress,
            int program,
            int[] versions,
            ServerSettings serverSettings = default)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            if ((versions == null) || (versions.Length == 0))
            {
                throw new ArgumentNullException(nameof(versions));
            }

            if (protocol.HasFlag(Protocol.Tcp))
            {
                this.rpcTcpServer = new RpcTcpServer(
                    ipAddress,
                    program,
                    versions,
                    this.DispatchReceivedCall,
                    serverSettings);
            }

            if (protocol.HasFlag(Protocol.Udp))
            {
                this.rpcUdpServer = new RpcUdpServer(
                    ipAddress,
                    program,
                    versions,
                    this.DispatchReceivedCall,
                    serverSettings);
            }
        }

        public void Start()
        {
            this.rpcTcpServer?.Start();
            this.rpcUdpServer?.Start();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void DispatchReceivedCall(ReceivedRpcCall call);

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.rpcUdpServer?.Dispose();
                    this.rpcTcpServer?.Dispose();
                }

                this.isDisposed = true;
            }
        }
    }
}
