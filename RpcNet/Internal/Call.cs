namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    internal class Call
    {
        private readonly IPEndPoint remoteIpEndPoint;
        private readonly INetworkReader networkReader;
        private readonly INetworkWriter networkWriter;
        private readonly IXdrReader xdrReader;
        private readonly IXdrWriter xdrWriter;
        private readonly Random random = new Random();
        private readonly RpcMessage rpcMessage;
        private readonly ILogger logger;
        private readonly Action reestablishConnection;

        public Call(
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
                Xid = (uint)this.random.Next(),
                Body = new Body
                {
                    MessageType = MessageType.Call,
                    CallBody = new CallBody
                    {
                        RpcVersion = 2,
                        Program = (uint)program,
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

            this.rpcMessage.Xid = (uint)this.random.Next();
            this.rpcMessage.Body.CallBody.Procedure = (uint)procedure;
            this.rpcMessage.Body.CallBody.Version = (uint)version;
            this.rpcMessage.WriteTo(this.xdrWriter);
            argument.WriteTo(this.xdrWriter);

            NetworkResult networkResult = this.networkWriter.EndWriting();
            if (networkResult.SocketError != SocketError.Success)
            {
                errorMessage =
                    $"Could not send message to {this.remoteIpEndPoint}. Socket error: {networkResult.SocketError}.";
                return false;
            }

            networkResult = this.networkReader.BeginReading();
            if (networkResult.SocketError != SocketError.Success)
            {
                errorMessage =
                    $"Could not receive reply from {this.remoteIpEndPoint}. Socket error: {networkResult.SocketError}.";
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
                errorMessage =
                    $"Wrong message type. Expected {MessageType.Reply}, but was {reply.Body.MessageType}.";
                return false;
            }

            if (reply.Body.ReplyBody.ReplyStatus != ReplyStatus.Accepted)
            {
                errorMessage = $"Call was denied.";
                return false;
            }

            if (reply.Body.ReplyBody.AcceptedReply.ReplyData.AcceptStatus != AcceptStatus.Success)
            {
                errorMessage =
                    $"Call was unsuccessful: {reply.Body.ReplyBody.AcceptedReply.ReplyData.AcceptStatus}.";
                return false;
            }

            result.ReadFrom(this.xdrReader);
            this.networkReader.EndReading();

            errorMessage = null;
            return true;
        }
    }
}
