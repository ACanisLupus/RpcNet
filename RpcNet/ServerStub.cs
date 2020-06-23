namespace RpcNet
{
    using System;
    using System.Net;
    using RpcNet.Internal;

    public abstract class ServerStub : IDisposable
    {
        private readonly RpcUdpServer rpcUdpServer;
        private readonly RpcTcpServer rpcTcpServer;
        private bool isDisposed;

        public ServerStub(IPAddress ipAddress, int port, int program, int[] versions)
        {
            this.rpcUdpServer = new RpcUdpServer(ipAddress, port, program, versions, this.DispatchReceivedCall);
            this.rpcTcpServer = new RpcTcpServer(ipAddress, port, program, versions, this.DispatchReceivedCall);
        }

        protected abstract void DispatchReceivedCall(ReceivedCall call);

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.rpcUdpServer.Dispose();
                    this.rpcTcpServer.Dispose();
                }

                this.isDisposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
