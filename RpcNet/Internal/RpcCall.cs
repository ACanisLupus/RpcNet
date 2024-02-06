// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;

internal sealed class RpcCall
{
    private readonly ILogger? _logger;
    private readonly INetworkReader _networkReader;
    private readonly INetworkWriter _networkWriter;
    private readonly Action? _reestablishConnection;
    private readonly IPEndPoint _remoteIpEndPoint;
    private readonly RpcMessage _rpcMessage;
    private readonly IXdrReader _xdrReader;
    private readonly IXdrWriter _xdrWriter;

    private uint _nextXid = (uint)new Random().Next();

    public RpcCall(
        int program,
        IPEndPoint remoteIpEndPoint,
        INetworkReader networkReader,
        INetworkWriter networkWriter,
        Action? reestablishConnection,
        ILogger? logger = default)
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
        _logger = logger;
        _reestablishConnection = reestablishConnection;
    }

    public void SendCall(int procedure, int version, IXdrDataType argument, IXdrDataType result)
    {
        for (int i = 0; i < 2; i++)
        {
            if (!SendMessage(procedure, version, argument, out string? errorMessage))
            {
                if (i == 0)
                {
                    _logger?.Error(errorMessage + " Retrying...");
                    _reestablishConnection?.Invoke();
                    continue;
                }

                errorMessage ??= "Unknown error.";
                throw new RpcException(errorMessage);
            }

            if (!ReceiveReply(result, out errorMessage))
            {
                errorMessage ??= "Unknown error.";
                throw new RpcException(errorMessage);
            }

            break;
        }
    }

    private bool SendMessage(int procedure, int version, IXdrDataType argument, out string? errorMessage)
    {
        _networkWriter.BeginWriting();

        _rpcMessage.Xid = _nextXid++;
        _rpcMessage.Body.CallBody.Procedure = (uint)procedure;
        _rpcMessage.Body.CallBody.Version = (uint)version;
        _rpcMessage.WriteTo(_xdrWriter);
        argument.WriteTo(_xdrWriter);

        NetworkWriteResult writeResult = _networkWriter.EndWriting(_remoteIpEndPoint);
        if (writeResult.HasError)
        {
            errorMessage = $"Could not send message to {_remoteIpEndPoint}. Socket error: {writeResult.SocketError}.";
            return false;
        }

        NetworkReadResult readResult = _networkReader.BeginReading();
        if (readResult.HasError)
        {
            errorMessage = $"Could not receive reply from {_remoteIpEndPoint}. Socket error: {readResult.SocketError}.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    private bool ReceiveReply(IXdrDataType result, out string? errorMessage)
    {
        var reply = new RpcMessage();
        reply.ReadFrom(_xdrReader);

        if (reply.Xid != _rpcMessage.Xid)
        {
            errorMessage = $"Wrong XID. Expected {_rpcMessage.Xid}, but was {reply.Xid}.";
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

        result.ReadFrom(_xdrReader);
        _networkReader.EndReading();

        errorMessage = null;
        return true;
    }
}
