using System.Net;

namespace RpcNet
{
    public class ReceivedCall
    {
        public int Version { get; }
        public int Procedure { get; }
        public IPEndPoint RemoteIpEndPoint { get; }

        public void RetrieveCall(IXdrReadable argument)
        {

        }

        public void Reply(IXdrWritable result)
        {

        }

        public void ProcedureUnavailable()
        {

        }

        public void ProgramMismatch()
        {

        }
    }
}
