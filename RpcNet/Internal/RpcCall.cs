namespace RpcNet.Internal
{
    using System;
    using System.Net;

    internal class RpcCall
    {
        private readonly ILogger logger;
        private readonly INetworkReader networkReader;
        private readonly INetworkWriter networkWriter;
        private readonly Action reestablishConnection;
        private readonly IPEndPoint remoteIpEndPoint;
        private readonly RpcMessage rpcMessage;
        private readonly IXdrReader xdrReader;
        private readonly IXdrWriter xdrWriter;

        private uint nextXid = (uint)new Random().Next();

        public RpcCall(
            int program,
            IPEndPoint remoteIpEndPoint,
            INetworkReader networkReader,
            INetworkWriter networkWriter,
            Action reestablishConnection,
            ILogger logger)
        {
            this.remoteIpEndPoint = remoteIpEndPoint;
            this.networkReader = networkReader;
            this.networkWriter = networkWriter;
            this.xdrReader = new XdrReader(networkReader);
            this.xdrWriter = new XdrWriter(networkWriter);
            this.rpcMessage = new RpcMessage
            {
                Xid = this.nextXid++,
                Body = new Body
                {
                    MessageType = MessageType.Call,
                    CallBody = new CallBody
                    {
                        RpcVersion = 2,
                        Program = (uint)program,
                        Credential = new OpaqueAuthentication
                            { AuthenticationFlavor = AuthenticationFlavor.None, Body = new byte[0] },
                        Verifier = new OpaqueAuthentication
                            { AuthenticationFlavor = AuthenticationFlavor.None, Body = new byte[0] }
                    }
                }
            };
            this.logger = logger;
            this.reestablishConnection = reestablishConnection;
        }

        public void SendCall(int procedure, int version, IXdrWritable argument, IXdrReadable result)
        {
            for (int i = 0; i < 2; i++)
            {
                if (!this.SendMessage(procedure, version, argument, out string errorMessage))
                {
                    if (i == 0)
                    {
                        this.logger?.Error(errorMessage + " Retrying...");
                        this.reestablishConnection?.Invoke();
                        continue;
                    }

                    this.logger?.Error(errorMessage);
                    throw new RpcException(errorMessage);
                }

                if (!this.ReceiveReply(result, out errorMessage))
                {
                    if (i == 0)
                    {
                        this.logger?.Error(errorMessage + " Retrying...");
                        this.reestablishConnection?.Invoke();
                        continue;
                    }

                    this.logger?.Error(errorMessage);
                    throw new RpcException(errorMessage);
                }

                break;
            }
        }

        private bool SendMessage(int procedure, int version, IXdrWritable argument, out string errorMessage)
        {
            this.networkWriter.BeginWriting();

            this.rpcMessage.Xid = this.nextXid++;
            this.rpcMessage.Body.CallBody.Procedure = (uint)procedure;
            this.rpcMessage.Body.CallBody.Version = (uint)version;
            this.rpcMessage.WriteTo(this.xdrWriter);
            argument.WriteTo(this.xdrWriter);

            NetworkWriteResult writeResult = this.networkWriter.EndWriting(this.remoteIpEndPoint);
            if (writeResult.HasError)
            {
                errorMessage =
                    $"Could not send message to {this.remoteIpEndPoint}. Socket error: {writeResult.SocketError}.";
                return false;
            }

            NetworkReadResult readResult = this.networkReader.BeginReading();
            if (readResult.HasError)
            {
                errorMessage =
                    $"Could not receive reply from {this.remoteIpEndPoint}. Socket error: {readResult.SocketError}.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private bool ReceiveReply(IXdrReadable result, out string errorMessage)
        {
            var reply = new RpcMessage();
            reply.ReadFrom(this.xdrReader);

            if (reply.Xid != this.rpcMessage.Xid)
            {
                errorMessage = $"Wrong XID. Expected {this.rpcMessage.Xid}, but was {reply.Xid}.";
                return false;
            }

            if (reply.Body.MessageType != MessageType.Reply)
            {
                errorMessage = $"Wrong message type. Expected {MessageType.Reply}, but was {reply.Body.MessageType}.";
                return false;
            }

            if (reply.Body.ReplyBody.ReplyStatus != ReplyStatus.Accepted)
            {
                errorMessage = "Call was denied.";
                return false;
            }

            if (reply.Body.ReplyBody.AcceptedReply.ReplyData.AcceptStatus != AcceptStatus.Success)
            {
                errorMessage = $"Call was unsuccessful: {reply.Body.ReplyBody.AcceptedReply.ReplyData.AcceptStatus}.";
                return false;
            }

            result.ReadFrom(this.xdrReader);
            this.networkReader.EndReading();

            errorMessage = null;
            return true;
        }
    }
}
