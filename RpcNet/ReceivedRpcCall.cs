// Copyright by Artur Wolf

namespace RpcNet;

using RpcNet.Internal;

public sealed class ReceivedRpcCall
{
    private readonly uint _highVersion;
    private readonly uint _lowVersion;
    private readonly int _program;
    private readonly Action<ReceivedRpcCall> _receivedCallDispatcher;
    private readonly IXdrReader _xdrReader;
    private readonly IXdrWriter _xdrWriter;

    private uint _xid;

    internal ReceivedRpcCall(
        int program,
        int[] versions,
        INetworkReader networkReader,
        INetworkWriter networkWriter,
        Action<ReceivedRpcCall> receivedCallDispatcher)
    {
        _program = program;
        _receivedCallDispatcher = receivedCallDispatcher;
        _lowVersion = (uint)versions.Min();
        _highVersion = (uint)versions.Max();
        _xdrReader = new XdrReader(networkReader);
        _xdrWriter = new XdrWriter(networkWriter);
    }

    public uint Version { get; private set; }
    public uint Procedure { get; private set; }
    public RpcEndPoint? RpcEndPoint { get; private set; }

    public void RetrieveCall(IXdrDataType argument) => argument.ReadFrom(_xdrReader);

    public void Reply(IXdrDataType result)
    {
        RpcMessage reply = GenerateReply(AcceptStatus.Success);
        reply.WriteTo(_xdrWriter);
        result.WriteTo(_xdrWriter);
    }

    public void ProcedureUnavailable()
    {
        RpcMessage reply = GenerateReply(AcceptStatus.ProcedureUnavailable);
        reply.WriteTo(_xdrWriter);
    }

    public void SystemError()
    {
        RpcMessage reply = GenerateReply(AcceptStatus.SystemError);
        reply.WriteTo(_xdrWriter);
    }

    public void ProgramMismatch()
    {
        RpcMessage reply = GenerateProgramMismatch(_lowVersion, _highVersion);
        reply.WriteTo(_xdrWriter);
    }

    internal void HandleCall(RpcEndPoint rpcEndPoint)
    {
        var rpcMessage = new RpcMessage(_xdrReader);
        _xid = rpcMessage.Xid;
        if (rpcMessage.Body.MessageType != MessageType.Call)
        {
            throw new RpcException($"Message type should be {nameof(MessageType.Call)} but was {rpcMessage.Body.MessageType}.");
        }

        if (rpcMessage.Body.CallBody.RpcVersion != Utilities.RpcVersion)
        {
            RpcMessage reply = GenerateRpcVersionMismatch(Utilities.RpcVersion, Utilities.RpcVersion);
            reply.WriteTo(_xdrWriter);
            return;
        }

        if (rpcMessage.Body.CallBody.Program != _program)
        {
            RpcMessage reply = GenerateReply(AcceptStatus.ProgramUnavailable);
            reply.WriteTo(_xdrWriter);
            return;
        }

        Version = rpcMessage.Body.CallBody.Version;
        Procedure = rpcMessage.Body.CallBody.Procedure;
        RpcEndPoint = rpcEndPoint;

        _receivedCallDispatcher(this);
    }

    private RpcMessage GenerateReply(ReplyBody replyBody) => new()
    {
        Xid = _xid,
        Body =
        {
            MessageType = MessageType.Reply,
            ReplyBody = replyBody
        }
    };

    private RpcMessage GenerateReply(RejectedReply rejectedReply) =>
        GenerateReply(
            new ReplyBody
            {
                ReplyStatus = ReplyStatus.Denied,
                RejectedReply = rejectedReply
            });

    private RpcMessage GenerateRpcVersionMismatch(uint low, uint high) => GenerateReply(
        new RejectedReply
        {
            RejectStatus = RejectStatus.RpcVersionMismatch,
            MismatchInfo = new MismatchInfo
            {
                High = high,
                Low = low
            }
        });

    private RpcMessage GenerateReply(ReplyData replyData) =>
        GenerateReply(
            new ReplyBody
            {
                ReplyStatus = ReplyStatus.Accepted,
                AcceptedReply = new AcceptedReply
                {
                    Verifier = new OpaqueAuthentication
                    {
                        AuthenticationFlavor = AuthenticationFlavor.None,
                        Body = Array.Empty<byte>()
                    },
                    ReplyData = replyData
                }
            });

    private RpcMessage GenerateReply(AcceptStatus acceptStatus) => GenerateReply(
        new ReplyData
        {
            AcceptStatus = acceptStatus
        });

    private RpcMessage GenerateProgramMismatch(uint low, uint high) => GenerateReply(
        new ReplyData
        {
            AcceptStatus = AcceptStatus.ProgramMismatch,
            MismatchInfo = new MismatchInfo
            {
                Low = low,
                High = high
            }
        });
}
