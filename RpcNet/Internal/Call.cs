namespace RpcNet.Internal
{
    using System;

    public class Call
    {
        private readonly IXdrReader xdrReader;
        private readonly IXdrWriter xdrWriter;
        private readonly Random random = new Random();
        private readonly RpcMessage rpcMessage;

        public Call(uint program, INetworkReader networkReader, INetworkWriter networkWriter)
        {
            this.xdrReader = new XdrReader(networkReader);
            this.xdrWriter = new XdrWriter(networkWriter);
            this.rpcMessage = new RpcMessage
            {
                Xid = (uint)this.random.Next(),
                Body = new Body
                {
                    MessageType = MessageType.Call,
                    CallBody = new CallBody
                    {
                        RpcVersion = 2,
                        Program = program,
                        Credential = new OpaqueAuthentication
                        {
                            AuthenticationFlavor = AuthenticationFlavor.None,
                            Body = new byte[0]
                        },
                        Verifier = new OpaqueAuthentication
                        {
                            AuthenticationFlavor = AuthenticationFlavor.None,
                            Body = new byte[0]
                        }
                    }
                }
            };
        }

        public void SendCall(uint procedure, uint version, IXdrWritable argument)
        {
            this.rpcMessage.Xid = (uint)this.random.Next();
            this.rpcMessage.Body.CallBody.Procedure = procedure;
            this.rpcMessage.Body.CallBody.Version = version;

            this.rpcMessage.WriteTo(this.xdrWriter);
            argument.WriteTo(this.xdrWriter);
        }

        public void ReceiveResult(IXdrReadable result)
        {
            var reply = new RpcMessage();
            reply.ReadFrom(this.xdrReader);

            if (reply.Xid != this.rpcMessage.Xid)
            {
                throw new RpcException($"Wrong XID. Expected {this.rpcMessage.Xid}, but was {reply.Xid}.");
            }

            if (reply.Body.MessageType != MessageType.Reply)
            {
                throw new RpcException($"Wrong message type. Expected {MessageType.Reply}, but was {reply.Body.MessageType}.");
            }

            if (reply.Body.ReplyBody.ReplyStatus != ReplyStatus.Accepted)
            {
                throw new RpcException($"Call was denied.");
            }

            if (reply.Body.ReplyBody.AcceptedReply.ReplyData.AcceptStatus != AcceptStatus.Success)
            {
                throw new RpcException($"Call was unsuccessful: {reply.Body.ReplyBody.AcceptedReply.ReplyData.AcceptStatus}.");
            }

            result.ReadFrom(this.xdrReader);
        }
    }
}
