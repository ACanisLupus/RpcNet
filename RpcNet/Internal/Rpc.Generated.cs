//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by RpcNetGen 1.0.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RpcNet.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using RpcNet;

    internal static class RpcConstants
    {
    }

    internal enum AcceptStatus
    {
        Success = 0,
        ProgramUnavailable = 1,
        ProgramMismatch = 2,
        ProcedureUnavailable = 3,
        GarbageArguments = 4,
        SystemError = 5,
    }

    internal enum AuthenticationFlavor
    {
        None = 0,
        Unix = 1,
        Short = 2,
        Des = 3,
        Gss = 6,
    }

    internal enum AuthenticationStatus
    {
        Ok = 0,
        BadCredential = 1,
        RejectedCredential = 2,
        BadVerifier = 3,
        RejectedVerifier = 4,
        TooWeak = 5,
        InvalidResponseVerifier = 6,
        FailedUnknownReason = 7,
        KerberosGenericError = 8,
        TimeOfCredentialExpired = 9,
        ProblemWithTicketFile = 10,
        FailedToDecodeAuthenticator = 11,
        InvalidNetAddress = 12,
        GssMissingCredential = 13,
        GssContextProblem = 14,
    }

    internal enum MessageType
    {
        Call = 0,
        Reply = 1,
    }

    internal enum RejectStatus
    {
        RpcVersionMismatch = 0,
        AuthenticationError = 1,
    }

    internal enum ReplyStatus
    {
        Accepted = 0,
        Denied = 1,
    }

    internal partial class AcceptedReply : IXdrDataType
    {
        public AcceptedReply()
        {
        }

        public AcceptedReply(IXdrReader reader)
        {
            ReadFrom(reader);
        }

        public OpaqueAuthentication Verifier { get; set; } = new OpaqueAuthentication();
        public ReplyData ReplyData { get; set; } = new ReplyData();

        public void WriteTo(IXdrWriter writer)
        {
            Verifier.WriteTo(writer);
            ReplyData.WriteTo(writer);
        }

        public void ReadFrom(IXdrReader reader)
        {
            Verifier.ReadFrom(reader);
            ReplyData.ReadFrom(reader);
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append("{");
            sb.Append(" Verifier = ");
            Verifier.ToString(sb);
            sb.Append(", ReplyData = ");
            ReplyData.ToString(sb);
            sb.Append(" }");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
    }

    internal partial class CallBody : IXdrDataType
    {
        public CallBody()
        {
        }

        public CallBody(IXdrReader reader)
        {
            ReadFrom(reader);
        }

        public uint RpcVersion { get; set; }
        public uint Program { get; set; }
        public uint Version { get; set; }
        public uint Procedure { get; set; }
        public OpaqueAuthentication Credential { get; set; } = new OpaqueAuthentication();
        public OpaqueAuthentication Verifier { get; set; } = new OpaqueAuthentication();

        public void WriteTo(IXdrWriter writer)
        {
            writer.Write(RpcVersion);
            writer.Write(Program);
            writer.Write(Version);
            writer.Write(Procedure);
            Credential.WriteTo(writer);
            Verifier.WriteTo(writer);
        }

        public void ReadFrom(IXdrReader reader)
        {
            RpcVersion = reader.ReadUInt32();
            Program = reader.ReadUInt32();
            Version = reader.ReadUInt32();
            Procedure = reader.ReadUInt32();
            Credential.ReadFrom(reader);
            Verifier.ReadFrom(reader);
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append("{");
            sb.Append(" RpcVersion = ");
            sb.Append(RpcVersion);
            sb.Append(", Program = ");
            sb.Append(Program);
            sb.Append(", Version = ");
            sb.Append(Version);
            sb.Append(", Procedure = ");
            sb.Append(Procedure);
            sb.Append(", Credential = ");
            Credential.ToString(sb);
            sb.Append(", Verifier = ");
            Verifier.ToString(sb);
            sb.Append(" }");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
    }

    internal partial class MismatchInfo : IXdrDataType
    {
        public MismatchInfo()
        {
        }

        public MismatchInfo(IXdrReader reader)
        {
            ReadFrom(reader);
        }

        public uint Low { get; set; }
        public uint High { get; set; }

        public void WriteTo(IXdrWriter writer)
        {
            writer.Write(Low);
            writer.Write(High);
        }

        public void ReadFrom(IXdrReader reader)
        {
            Low = reader.ReadUInt32();
            High = reader.ReadUInt32();
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append("{");
            sb.Append(" Low = ");
            sb.Append(Low);
            sb.Append(", High = ");
            sb.Append(High);
            sb.Append(" }");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
    }

    internal partial class OpaqueAuthentication : IXdrDataType
    {
        public OpaqueAuthentication()
        {
        }

        public OpaqueAuthentication(IXdrReader reader)
        {
            ReadFrom(reader);
        }

        public AuthenticationFlavor AuthenticationFlavor { get; set; }
        public byte[] Body { get; set; }

        public void WriteTo(IXdrWriter writer)
        {
            writer.Write((int)AuthenticationFlavor);
            writer.WriteOpaque(Body);
        }

        public void ReadFrom(IXdrReader reader)
        {
            AuthenticationFlavor = (AuthenticationFlavor)reader.ReadInt32();
            Body = reader.ReadOpaque();
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append("{");
            sb.Append(" AuthenticationFlavor = ");
            sb.Append(AuthenticationFlavor);
            sb.Append(", Body = [");
            for (int _idx = 0; _idx < Body.Length; _idx++)
            {
                sb.Append(_idx == 0 ? " " : ", ");
                sb.Append(Body[_idx]);
            }
            sb.Append(" ]");
            sb.Append(" }");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
    }

    internal partial class RpcMessage : IXdrDataType
    {
        public RpcMessage()
        {
        }

        public RpcMessage(IXdrReader reader)
        {
            ReadFrom(reader);
        }

        public uint Xid { get; set; }
        public Body Body { get; set; } = new Body();

        public void WriteTo(IXdrWriter writer)
        {
            writer.Write(Xid);
            Body.WriteTo(writer);
        }

        public void ReadFrom(IXdrReader reader)
        {
            Xid = reader.ReadUInt32();
            Body.ReadFrom(reader);
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append("{");
            sb.Append(" Xid = ");
            sb.Append(Xid);
            sb.Append(", Body = ");
            Body.ToString(sb);
            sb.Append(" }");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
    }

    internal partial class Body : IXdrDataType
    {
        public Body()
        {
        }

        public Body(IXdrReader reader)
        {
            ReadFrom(reader);
        }

        public MessageType MessageType { get; set; }
        public CallBody CallBody { get; set; } = new CallBody();
        public ReplyBody ReplyBody { get; set; } = new ReplyBody();

        public void WriteTo(IXdrWriter writer)
        {
            writer.Write((int)MessageType);
            switch (MessageType)
            {
                case MessageType.Call:
                    CallBody.WriteTo(writer);
                    break;
                case MessageType.Reply:
                    ReplyBody.WriteTo(writer);
                    break;
            }
        }

        public void ReadFrom(IXdrReader reader)
        {
            MessageType = (MessageType)reader.ReadInt32();
            switch (MessageType)
            {
                case MessageType.Call:
                    CallBody.ReadFrom(reader);
                    break;
                case MessageType.Reply:
                    ReplyBody.ReadFrom(reader);
                    break;
            }
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append("{");
            switch (MessageType)
            {
                case MessageType.Call:
                    sb.Append(" CallBody = ");
                    CallBody.ToString(sb);
                    break;
                case MessageType.Reply:
                    sb.Append(" ReplyBody = ");
                    ReplyBody.ToString(sb);
                    break;
            }
            sb.Append(" }");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
    }

    internal partial class RejectedReply : IXdrDataType
    {
        public RejectedReply()
        {
        }

        public RejectedReply(IXdrReader reader)
        {
            ReadFrom(reader);
        }

        public RejectStatus RejectStatus { get; set; }
        public MismatchInfo MismatchInfo { get; set; } = new MismatchInfo();
        public AuthenticationStatus AuthenticationStatus { get; set; }

        public void WriteTo(IXdrWriter writer)
        {
            writer.Write((int)RejectStatus);
            switch (RejectStatus)
            {
                case RejectStatus.RpcVersionMismatch:
                    MismatchInfo.WriteTo(writer);
                    break;
                case RejectStatus.AuthenticationError:
                    writer.Write((int)AuthenticationStatus);
                    break;
            }
        }

        public void ReadFrom(IXdrReader reader)
        {
            RejectStatus = (RejectStatus)reader.ReadInt32();
            switch (RejectStatus)
            {
                case RejectStatus.RpcVersionMismatch:
                    MismatchInfo.ReadFrom(reader);
                    break;
                case RejectStatus.AuthenticationError:
                    AuthenticationStatus = (AuthenticationStatus)reader.ReadInt32();
                    break;
            }
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append("{");
            switch (RejectStatus)
            {
                case RejectStatus.RpcVersionMismatch:
                    sb.Append(" MismatchInfo = ");
                    MismatchInfo.ToString(sb);
                    break;
                case RejectStatus.AuthenticationError:
                    sb.Append(" AuthenticationStatus = ");
                    sb.Append(AuthenticationStatus);
                    break;
            }
            sb.Append(" }");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
    }

    internal partial class ReplyBody : IXdrDataType
    {
        public ReplyBody()
        {
        }

        public ReplyBody(IXdrReader reader)
        {
            ReadFrom(reader);
        }

        public ReplyStatus ReplyStatus { get; set; }
        public AcceptedReply AcceptedReply { get; set; } = new AcceptedReply();
        public RejectedReply RejectedReply { get; set; } = new RejectedReply();

        public void WriteTo(IXdrWriter writer)
        {
            writer.Write((int)ReplyStatus);
            switch (ReplyStatus)
            {
                case ReplyStatus.Accepted:
                    AcceptedReply.WriteTo(writer);
                    break;
                case ReplyStatus.Denied:
                    RejectedReply.WriteTo(writer);
                    break;
            }
        }

        public void ReadFrom(IXdrReader reader)
        {
            ReplyStatus = (ReplyStatus)reader.ReadInt32();
            switch (ReplyStatus)
            {
                case ReplyStatus.Accepted:
                    AcceptedReply.ReadFrom(reader);
                    break;
                case ReplyStatus.Denied:
                    RejectedReply.ReadFrom(reader);
                    break;
            }
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append("{");
            switch (ReplyStatus)
            {
                case ReplyStatus.Accepted:
                    sb.Append(" AcceptedReply = ");
                    AcceptedReply.ToString(sb);
                    break;
                case ReplyStatus.Denied:
                    sb.Append(" RejectedReply = ");
                    RejectedReply.ToString(sb);
                    break;
            }
            sb.Append(" }");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
    }

    internal partial class ReplyData : IXdrDataType
    {
        public ReplyData()
        {
        }

        public ReplyData(IXdrReader reader)
        {
            ReadFrom(reader);
        }

        public AcceptStatus AcceptStatus { get; set; }
        public MismatchInfo MismatchInfo { get; set; } = new MismatchInfo();

        public void WriteTo(IXdrWriter writer)
        {
            writer.Write((int)AcceptStatus);
            switch (AcceptStatus)
            {
                case AcceptStatus.Success:
                    break;
                case AcceptStatus.ProgramMismatch:
                    MismatchInfo.WriteTo(writer);
                    break;
                default:
                    break;
            }
        }

        public void ReadFrom(IXdrReader reader)
        {
            AcceptStatus = (AcceptStatus)reader.ReadInt32();
            switch (AcceptStatus)
            {
                case AcceptStatus.Success:
                    break;
                case AcceptStatus.ProgramMismatch:
                    MismatchInfo.ReadFrom(reader);
                    break;
                default:
                    break;
            }
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append("{");
            switch (AcceptStatus)
            {
                case AcceptStatus.Success:
                    break;
                case AcceptStatus.ProgramMismatch:
                    sb.Append(" MismatchInfo = ");
                    MismatchInfo.ToString(sb);
                    break;
                default:
                    break;
            }
            sb.Append(" }");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
    }
}