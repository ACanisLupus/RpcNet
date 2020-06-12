namespace RpcNet
{
    using System.Net;
    using RpcNet.Internal;

    public class ReceivedCall
    {
        private readonly IXdrReader xdrReader;
        private readonly IXdrWriter xdrWriter;
        private uint xid;

        public ReceivedCall(IPEndPoint ipEndPoint, IXdrReader xdrReader, IXdrWriter xdrWriter)
        {
            this.RemoteIpEndPoint = ipEndPoint;
            this.xdrReader = xdrReader;
            this.xdrWriter = xdrWriter;
        }

        public uint Version { get; private set; }
        public uint Procedure { get; private set; }
        public IPEndPoint RemoteIpEndPoint { get; }

        public void RetrieveCall(IXdrReadable argument)
        {
            var rpcMessage = new RpcMessage(this.xdrReader);
            this.xid = rpcMessage.Xid;
            if (rpcMessage.Body.MessageType != MessageType.Call)
            {
                throw new RpcException($"Message type should be {nameof(MessageType.Call)} but was {rpcMessage.Body.MessageType}.");
            }

            if (rpcMessage.Body.CallBody.RpcVersion != 2)
            {
                throw new RpcException($"RPC version should be 2 but was {rpcMessage.Body.CallBody.RpcVersion}.");
            }

            this.Version = rpcMessage.Body.CallBody.Version;
            this.Procedure = rpcMessage.Body.CallBody.Procedure;

            argument.ReadFrom(this.xdrReader);
        }

        public void Reply(IXdrWritable result)
        {
            // TODO: Implement
            result.WriteTo(this.xdrWriter);
        }

        public void ProcedureUnavailable()
        {
            // TODO: Implement
        }

        public void ProgramMismatch()
        {
            // TODO: Implement
        }
    }
}
