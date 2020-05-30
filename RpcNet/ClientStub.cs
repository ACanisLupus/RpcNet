using System.Net;

namespace RpcNet
{
    public abstract class ClientStub
    {
        protected ClientStub(IPAddress ipAddress, int port, int program, int version)
        {
        }

        public void Call(int procedure, int version, IXdrWritable argument, IXdrReadable result)
        {
        }
    }
}
