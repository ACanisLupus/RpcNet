namespace RpcNet
{
    using System.Net;

    public abstract class ClientStub
    {
        protected ClientStub(Protocol protocol, IPAddress ipAddress, int port, int program, int version)
        {
        }

        public void Call(int procedure, int version, IXdrWritable argument, IXdrReadable result)
        {
        }
    }
}
