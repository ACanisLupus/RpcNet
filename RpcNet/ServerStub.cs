namespace RpcNet
{
    using System.Net;

    public abstract class ServerStub
    {
        public ServerStub(IPAddress ipAddress, int port, int program, int[] versions)
        {
        }

        protected abstract void DispatchReceivedCall(ReceivedCall call);
    }
}
