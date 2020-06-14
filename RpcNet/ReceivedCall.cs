namespace RpcNet
{
    using System;
    using System.Linq;
    using System.Net;
    using RpcNet.Internal;

    public class ReceivedCall
    {
        private readonly IXdrReader xdrReader;
        private readonly IXdrWriter xdrWriter;
        private readonly int program;
        private readonly uint lowVersion;
        private readonly uint highVersion;
        private readonly Action<ReceivedCall> receivedCallDispatcher;

        private uint xid;

        public ReceivedCall(
            int program,
            int[] versions,
            INetworkReader networkReader,
            INetworkWriter networkWriter,
            Action<ReceivedCall> receivedCallDispatcher)
        {
            this.program = program;
            this.receivedCallDispatcher = receivedCallDispatcher;
            this.lowVersion = (uint)versions.Min();
            this.highVersion = (uint)versions.Max();
            this.xdrReader = new XdrReader(networkReader);
            this.xdrWriter = new XdrWriter(networkWriter);
        }

        public uint Version { get; private set; }
        public uint Procedure { get; private set; }
        public IPEndPoint RemoteIpEndPoint { get; private set; }

        public void HandleCall(IPEndPoint remoteIpEndPoint)
        {
            var rpcMessage = new RpcMessage(this.xdrReader);
            this.xid = rpcMessage.Xid;
            if (rpcMessage.Body.MessageType != MessageType.Call)
            {
                throw new RpcException($"Message type should be {nameof(MessageType.Call)} but was {rpcMessage.Body.MessageType}.");
            }

            if (rpcMessage.Body.CallBody.RpcVersion != 2)
            {
                RpcMessage reply = this.GenerateRpcVersionMismatch(2, 2);
                reply.WriteTo(this.xdrWriter);
                return;
            }

            if (rpcMessage.Body.CallBody.Program != this.program)
            {
                RpcMessage reply = this.GenerateReply(AcceptStatus.ProgramUnavailable);
                reply.WriteTo(this.xdrWriter);
                return;
            }

            this.Version = rpcMessage.Body.CallBody.Version;
            this.Procedure = rpcMessage.Body.CallBody.Procedure;
            this.RemoteIpEndPoint = remoteIpEndPoint;

            this.receivedCallDispatcher(this);
        }

        public void RetrieveCall(IXdrReadable argument)
        {
            argument.ReadFrom(this.xdrReader);
        }

        public void Reply(IXdrWritable result)
        {
            RpcMessage reply = this.GenerateReply(AcceptStatus.Success);
            reply.WriteTo(this.xdrWriter);
            result.WriteTo(this.xdrWriter);
        }

        public void ProcedureUnavailable()
        {
            RpcMessage reply = this.GenerateReply(AcceptStatus.ProcedureUnavailable);
            reply.WriteTo(this.xdrWriter);
        }

        public void ProgramMismatch()
        {
            RpcMessage reply = this.GenerateProgramMismatch(this.lowVersion, this.highVersion);
            reply.WriteTo(this.xdrWriter);
        }

        private RpcMessage GenerateReply(ReplyBody replyBody) =>
            new RpcMessage
            {
                Xid = this.xid,
                Body = new Body
                {
                    MessageType = MessageType.Reply,
                    ReplyBody = replyBody
                }
            };

        private RpcMessage GenerateReply(RejectedReply rejectedReply) =>
            this.GenerateReply(new ReplyBody
            {
                ReplyStatus = ReplyStatus.Denied,
                RejectedReply = rejectedReply
            });

        private RpcMessage GenerateRpcVersionMismatch(uint low, uint high) =>
            this.GenerateReply(new RejectedReply
            {
                RejectStatus = RejectStatus.RpcVersionMismatch,
                MismatchInfo = new MismatchInfo
                {
                    High = high,
                    Low = low
                }
            });

        private RpcMessage GenerateReply(ReplyData replyData) =>
            this.GenerateReply(new ReplyBody
            {
                ReplyStatus = ReplyStatus.Accepted,
                AcceptedReply = new AcceptedReply
                {
                    Verifier = new OpaqueAuthentication
                    {
                        AuthenticationFlavor = AuthenticationFlavor.None,
                        Body = new byte[0]
                    },
                    ReplyData = replyData
                }
            });

        private RpcMessage GenerateReply(AcceptStatus acceptStatus) =>
            this.GenerateReply(new ReplyData
            {
                AcceptStatus = acceptStatus
            });

        private RpcMessage GenerateProgramMismatch(uint low, uint high) =>
            this.GenerateReply(new ReplyData
            {
                AcceptStatus = AcceptStatus.ProgramMismatch,
                MismatchInfo = new MismatchInfo
                {
                    Low = low,
                    High = high
                }
            });
    }
}
