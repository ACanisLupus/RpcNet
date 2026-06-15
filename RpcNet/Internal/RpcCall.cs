// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;

internal sealed class RpcCall(int program, EndPoint remoteEndPoint, INetworkReader networkReader, INetworkWriter networkWriter)
{
    private readonly RpcMessage _rpcMessage = new()
    {
        Body =
        {
            MessageType = MessageType.Call,
            CallBody =
            {
                RpcVersion = Utilities.RpcVersion,
                Program = (uint)program,
                Credential =
                {
                    AuthenticationFlavor = AuthenticationFlavor.None,
                    Body = []
                },
                Verifier =
                {
                    AuthenticationFlavor = AuthenticationFlavor.None,
                    Body = []
                }
            }
        }
    };

    private readonly IXdrReader _xdrReader = new XdrReader(networkReader);
    private readonly IXdrWriter _xdrWriter = new XdrWriter(networkWriter);

    private uint _nextXid = (uint)new Random().Next();

    public async ValueTask SendCallAsync(int procedure, int version, IXdrDataType argument, IXdrDataType result, CancellationToken cancellationToken)
    {
        await SendMessageAsync(procedure, version, argument, cancellationToken).ConfigureAwait(false);
        await ReceiveReplyAsync(result, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask SendMessageAsync(int procedure, int version, IXdrDataType argument, CancellationToken cancellationToken)
    {
        networkWriter.BeginWriting();

        _rpcMessage.Xid = _nextXid++;
        _rpcMessage.Body.CallBody.Procedure = (uint)procedure;
        _rpcMessage.Body.CallBody.Version = (uint)version;
        _rpcMessage.WriteTo(_xdrWriter);
        argument.WriteTo(_xdrWriter);

        await networkWriter.EndWritingAsync(remoteEndPoint, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask ReceiveReplyAsync(IXdrDataType result, CancellationToken cancellationToken)
    {
        _ = await networkReader.BeginReadingAsync(cancellationToken).ConfigureAwait(false);

        RpcMessage reply = new();
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
        networkReader.EndReading();
    }
}
