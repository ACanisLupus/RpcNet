//namespace RpcNet.Internal
//{
//    using System;
//    using System.Net;
//    using System.Net.Sockets;

//    public class RpcUdpClient
//    {
//        private readonly byte[] buffer = new byte[65536];
//        private readonly UdpClient client;
//        private readonly XdrWriter writer;
//        private readonly XdrReader reader;
//        private readonly Random random = new Random();
//        private readonly RpcMessage rpcMessage;

//        public RpcUdpClient(IPAddress ipAddress, int port, uint program, uint version)
//        {
//            this.client = new UdpClient(new IPEndPoint(ipAddress, port));
//            this.writer = new XdrWriter(this.buffer);
//            this.reader = new XdrReader(this.buffer);
//            this.rpcMessage = new RpcMessage
//            {
//                Xid = (uint)this.random.Next(),
//                Body = new Body
//                {
//                    MessageType = MessageType.Call,
//                    CallBody = new CallBody
//                    {
//                        RpcVersion = 2,
//                        Program = program,
//                        Version = version,
//                        Credential = new OpaqueAuthentication
//                        {
//                            AuthenticationFlavor = AuthenticationFlavor.AUTH_NONE,
//                            Body = new byte[0]
//                        },
//                        Verifier = new OpaqueAuthentication
//                        {
//                            AuthenticationFlavor = AuthenticationFlavor.AUTH_NONE,
//                            Body = new byte[0]
//                        }
//                    }
//                }
//            };
//        }

//        public void Call(uint procedure, uint version, IXdrWritable argument, IXdrReadable result)
//        {
//            this.rpcMessage.Xid = (uint)this.random.Next();
//            this.rpcMessage.Body.CallBody.Procedure = procedure;
//            this.rpcMessage.Body.CallBody.Version = version;

//            this.writer.Reset();
//            rpcMessage.WriteTo(this.writer);
//            argument.WriteTo(this.writer);

//            this.client.Client.Send(this.buffer, 0, this.writer.Length, SocketFlags.None, out SocketError socketError);
//            if (socketError != SocketError.Success)
//            {
//                throw new RpcException($"Could not send UDP message. Socket error code: {socketError}.");
//            }

//            int length = this.client.Client.Receive(this.buffer, 0, this.buffer.Length, SocketFlags.None, out socketError);
//            if (socketError != SocketError.Success)
//            {
//                throw new RpcException($"Could not receive UDP message. Socket error code: {socketError}.");
//            }

//            this.reader.Reset(length);

//            var reply = new RpcMessage();
//            reply.ReadFrom(this.reader);
//            if (reply.Xid != this.rpcMessage.Xid)
//            {
//                throw new RpcException($"Wrong XID. Expected {this.rpcMessage.Xid}, but was {reply.Xid}.");
//            }

//            if (reply.Body.MessageType != MessageType.Reply)
//            {
//                throw new RpcException($"Wrong message type. Expected {MessageType.Reply}, but was {reply.Body.MessageType}.");
//            }

//            if (reply.Body.ReplyBody.ReplyStatus != ReplyStatus.Accepted)
//            {
//                throw new RpcException($"Call was denied.");
//            }

//            result.ReadFrom(this.reader);
//        }
//    }
//}
