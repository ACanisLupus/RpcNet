// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;

internal sealed class RpcCall
{
    private readonly INetworkReader _networkReader;
    private readonly INetworkWriter _networkWriter;
    private readonly IPEndPoint _remoteIpEndPoint;
    private readonly RpcMessage _rpcMessage;
    private readonly IXdrReader _xdrReader;
    private readonly IXdrWriter _xdrWriter;

    private uint _nextXid = (uint)new Random().Next();

    public RpcCall(int program, IPEndPoint remoteIpEndPoint, INetworkReader networkReader, INetworkWriter networkWriter)
    {
        _remoteIpEndPoint = remoteIpEndPoint;
        _networkReader = networkReader;
        _networkWriter = networkWriter;
        _xdrReader = new XdrReader(networkReader);
        _xdrWriter = new XdrWriter(networkWriter);
        _rpcMessage = new RpcMessage
        {
            Body =
            {
                MessageType = MessageType.Call,
                CallBody =
                {
                    RpcVersion = Utilities.RpcVersion,
                    Program = (uint)program,
                    Credential = { AuthenticationFlavor = AuthenticationFlavor.None, Body = Array.Empty<byte>() },
                    Verifier = { AuthenticationFlavor = AuthenticationFlavor.None, Body = Array.Empty<byte>() }
                }
            }
        };
    }

    public void SendCall(int procedure, int version, IXdrDataType argument, IXdrDataType result)
    {
        SendMessage(procedure, version, argument);
        ReceiveReply(result);
    }

    private void SendMessage(int procedure, int version, IXdrDataType argument)
    {
        _networkWriter.BeginWriting();

        _rpcMessage.Xid = _nextXid++;
        _rpcMessage.Body.CallBody.Procedure = (uint)procedure;
        _rpcMessage.Body.CallBody.Version = (uint)version;
        _rpcMessage.WriteTo(_xdrWriter);
        argument.WriteTo(_xdrWriter);

        _networkWriter.EndWriting(_remoteIpEndPoint);

        _ = _networkReader.BeginReading();
    }

    private void ReceiveReply(IXdrDataType result)
    {
        var reply = new RpcMessage();
        reply.ReadFrom(_xdrReader);

        if (reply.Xid != _rpcMessage.Xid)
        {
            throw new RpcException($"Wrong XID. Expected {_rpcMessage.Xid}, but was {reply.Xid}.");
        }

        if (reply.Body.MessageType != MessageType.Reply)
        {
            throw new RpcException($"Wrong message type. Expected {MessageType.Reply}, but was {reply.Body.MessageType}.");
        }

        if (reply.Body.ReplyBody.ReplyStatus != ReplyStatus.Accepted)
        {
            throw new RpcException("Call was denied.");
        }

        if (reply.Body.ReplyBody.AcceptedReply.ReplyData.AcceptStatus != AcceptStatus.Success)
        {
            throw new RpcException($"Call was unsuccessful: {reply.Body.ReplyBody.AcceptedReply.ReplyData.AcceptStatus}.");
        }

        result.ReadFrom(_xdrReader);
        _networkReader.EndReading();
    }
}
