namespace RpcNet
{
    using System;
    using System.Net;
    using RpcNet.Internal;

    public abstract class ServerStub : IDisposable
    {
        private readonly RpcUdpServer rpcUdpServer;

        public ServerStub(IPAddress ipAddress, int port, int program, int[] versions)
        {
            this.rpcUdpServer = new RpcUdpServer(ipAddress, port, program, versions, this.DispatchReceivedCall);
            this.rpcUdpServer.Start();
        }

        public void Dispose()
        {
            this.rpcUdpServer.Dispose();
        }

        protected abstract void DispatchReceivedCall(ReceivedCall call);
    }
}
