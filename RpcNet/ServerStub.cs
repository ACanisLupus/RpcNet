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

        protected ServerStub(IPAddress ipAddress, int port, int program, int[] versions, ILogger logger)
        {
            this.rpcUdpServer = new RpcUdpServer(ipAddress, port, program, versions, this.DispatchReceivedCall, logger);
            this.rpcTcpServer = new RpcTcpServer(ipAddress, port, program, versions, this.DispatchReceivedCall, logger);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
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
    }
}
