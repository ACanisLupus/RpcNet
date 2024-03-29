//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by RpcNetGen 2.0.4.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable

namespace TestService;

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using RpcNet;

internal static class TestService2Constants
{
    public const int Echo = 2;
    public const int NonExistingProcedure = 3;
    public const int NonExistingVersion = 3;
    public const int TestServiceProgram2 = 0x20406080;
    public const int TestServiceVersion = 1;
    public const int ThrowsException = 1;
}

internal class TestService2Client : ClientStub
{
    public TestService2Client(Protocol protocol, IPAddress ipAddress, int port = 0, ClientSettings? clientSettings = default) :
        base(protocol, ipAddress, port, TestService2Constants.TestServiceProgram2, TestService2Constants.NonExistingVersion, clientSettings)
    {
    }

    public void ThrowsException_1()
    {
        var args = Void;
        var result = Void;
        Settings?.Logger?.BeginCall(RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.ThrowsException, "ThrowsException_1", args);
        try
        {
            Call(TestService2Constants.ThrowsException, TestService2Constants.TestServiceVersion, args, result);
        }
        catch (Exception e)
        {
            Settings?.Logger?.EndCall(RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.ThrowsException, "ThrowsException_1", args, e);
            throw;
        }
        Settings?.Logger?.EndCall(RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.ThrowsException, "ThrowsException_1", args, result);
    }

    private class Echo_1_Arguments : IXdrDataType
    {
        public int Value { get; set; }

        public void WriteTo(IXdrWriter writer)
        {
            writer.Write(Value);
        }

        public void ReadFrom(IXdrReader reader)
        {
            Value = reader.ReadInt32();
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append("{");
            sb.Append(" Value = ");
            sb.Append(Value);
            sb.Append(" }");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
    }

    private class Echo_1_Result : IXdrDataType
    {
        public int Value { get; set; }

        public void WriteTo(IXdrWriter writer)
        {
            writer.Write(Value);
        }

        public void ReadFrom(IXdrReader reader)
        {
            Value = reader.ReadInt32();
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append("{");
            sb.Append(" Value = ");
            sb.Append(Value);
            sb.Append(" }");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
    }

    public int Echo_1(int value)
    {
        var args = new Echo_1_Arguments
        {
            Value = value,
        };
        var result = new Echo_1_Result();
        Settings?.Logger?.BeginCall(RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.Echo, "Echo_1", args);
        try
        {
            Call(TestService2Constants.Echo, TestService2Constants.TestServiceVersion, args, result);
        }
        catch (Exception e)
        {
            Settings?.Logger?.EndCall(RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.Echo, "Echo_1", args, e);
            throw;
        }
        Settings?.Logger?.EndCall(RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.Echo, "Echo_1", args, result);
        return result.Value;
    }

    private class NonExistingProcedure_1_Arguments : IXdrDataType
    {
        public byte[] SomeBytes { get; set; } = Array.Empty<byte>();

        public void WriteTo(IXdrWriter writer)
        {
            if (SomeBytes is null)
            {
                throw new InvalidOperationException("SomeBytes must not be null.");
            }
            writer.WriteOpaque(SomeBytes);
        }

        public void ReadFrom(IXdrReader reader)
        {
            SomeBytes = reader.ReadOpaque();
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append("{");
            if (SomeBytes is null)
            {
                sb.Append(" SomeBytes = null");
            }
            else
            {
                sb.Append(" SomeBytes = [");
                for (int _idx = 0; _idx < SomeBytes.Length; _idx++)
                {
                    sb.Append(_idx == 0 ? " " : ", ");
                    sb.Append(SomeBytes[_idx]);
                }
                sb.Append(" ]");
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

    public void NonExistingProcedure_1(byte[] someBytes)
    {
        var args = new NonExistingProcedure_1_Arguments
        {
            SomeBytes = someBytes,
        };
        var result = Void;
        Settings?.Logger?.BeginCall(RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.NonExistingProcedure, "NonExistingProcedure_1", args);
        try
        {
            Call(TestService2Constants.NonExistingProcedure, TestService2Constants.TestServiceVersion, args, result);
        }
        catch (Exception e)
        {
            Settings?.Logger?.EndCall(RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.NonExistingProcedure, "NonExistingProcedure_1", args, e);
            throw;
        }
        Settings?.Logger?.EndCall(RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.NonExistingProcedure, "NonExistingProcedure_1", args, result);
    }

    private class NonExistingProcedure_3_Arguments : IXdrDataType
    {
        public byte[] SomeBytes { get; set; } = Array.Empty<byte>();

        public void WriteTo(IXdrWriter writer)
        {
            if (SomeBytes is null)
            {
                throw new InvalidOperationException("SomeBytes must not be null.");
            }
            writer.WriteOpaque(SomeBytes);
        }

        public void ReadFrom(IXdrReader reader)
        {
            SomeBytes = reader.ReadOpaque();
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append("{");
            if (SomeBytes is null)
            {
                sb.Append(" SomeBytes = null");
            }
            else
            {
                sb.Append(" SomeBytes = [");
                for (int _idx = 0; _idx < SomeBytes.Length; _idx++)
                {
                    sb.Append(_idx == 0 ? " " : ", ");
                    sb.Append(SomeBytes[_idx]);
                }
                sb.Append(" ]");
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

    public void NonExistingProcedure_3(byte[] someBytes)
    {
        var args = new NonExistingProcedure_3_Arguments
        {
            SomeBytes = someBytes,
        };
        var result = Void;
        Settings?.Logger?.BeginCall(RpcEndPoint, TestService2Constants.NonExistingVersion, TestService2Constants.NonExistingProcedure, "NonExistingProcedure_3", args);
        try
        {
            Call(TestService2Constants.NonExistingProcedure, TestService2Constants.NonExistingVersion, args, result);
        }
        catch (Exception e)
        {
            Settings?.Logger?.EndCall(RpcEndPoint, TestService2Constants.NonExistingVersion, TestService2Constants.NonExistingProcedure, "NonExistingProcedure_3", args, e);
            throw;
        }
        Settings?.Logger?.EndCall(RpcEndPoint, TestService2Constants.NonExistingVersion, TestService2Constants.NonExistingProcedure, "NonExistingProcedure_3", args, result);
    }
}

internal abstract class TestService2ServerStub : ServerStub
{
    public TestService2ServerStub(Protocol protocol, IPAddress ipAddress, int port = 0, ServerSettings? serverSettings = default) :
        base(protocol, ipAddress, port, TestService2Constants.TestServiceProgram2, new[] { TestService2Constants.TestServiceVersion, TestService2Constants.NonExistingVersion }, serverSettings)
    {
    }

    private class Echo_1_Arguments : IXdrDataType
    {
        public int Value { get; set; }

        public void WriteTo(IXdrWriter writer)
        {
            writer.Write(Value);
        }

        public void ReadFrom(IXdrReader reader)
        {
            Value = reader.ReadInt32();
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append("{");
            sb.Append(" Value = ");
            sb.Append(Value);
            sb.Append(" }");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
    }

    private class Echo_1_Result : IXdrDataType
    {
        public int Value { get; set; }

        public void WriteTo(IXdrWriter writer)
        {
            writer.Write(Value);
        }

        public void ReadFrom(IXdrReader reader)
        {
            Value = reader.ReadInt32();
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append("{");
            sb.Append(" Value = ");
            sb.Append(Value);
            sb.Append(" }");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
    }

    private class NonExistingProcedure_1_Arguments : IXdrDataType
    {
        public byte[] SomeBytes { get; set; } = Array.Empty<byte>();

        public void WriteTo(IXdrWriter writer)
        {
            if (SomeBytes is null)
            {
                throw new InvalidOperationException("SomeBytes must not be null.");
            }
            writer.WriteOpaque(SomeBytes);
        }

        public void ReadFrom(IXdrReader reader)
        {
            SomeBytes = reader.ReadOpaque();
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append("{");
            if (SomeBytes is null)
            {
                sb.Append(" SomeBytes = null");
            }
            else
            {
                sb.Append(" SomeBytes = [");
                for (int _idx = 0; _idx < SomeBytes.Length; _idx++)
                {
                    sb.Append(_idx == 0 ? " " : ", ");
                    sb.Append(SomeBytes[_idx]);
                }
                sb.Append(" ]");
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

    private class NonExistingProcedure_3_Arguments : IXdrDataType
    {
        public byte[] SomeBytes { get; set; } = Array.Empty<byte>();

        public void WriteTo(IXdrWriter writer)
        {
            if (SomeBytes is null)
            {
                throw new InvalidOperationException("SomeBytes must not be null.");
            }
            writer.WriteOpaque(SomeBytes);
        }

        public void ReadFrom(IXdrReader reader)
        {
            SomeBytes = reader.ReadOpaque();
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append("{");
            if (SomeBytes is null)
            {
                sb.Append(" SomeBytes = null");
            }
            else
            {
                sb.Append(" SomeBytes = [");
                for (int _idx = 0; _idx < SomeBytes.Length; _idx++)
                {
                    sb.Append(_idx == 0 ? " " : ", ");
                    sb.Append(SomeBytes[_idx]);
                }
                sb.Append(" ]");
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

    public abstract void ThrowsException_1(RpcEndPoint rpcEndPoint);
    public abstract int Echo_1(RpcEndPoint rpcEndPoint, int value);
    public abstract void NonExistingProcedure_1(RpcEndPoint rpcEndPoint, byte[] someBytes);
    public abstract void NonExistingProcedure_3(RpcEndPoint rpcEndPoint, byte[] someBytes);

    protected override void DispatchReceivedCall(ReceivedRpcCall call)
    {
        if (call.Version == TestService2Constants.TestServiceVersion)
        {
            switch (call.Procedure)
            {
                case TestService2Constants.ThrowsException:
                {
                    var args = Void;
                    call.RetrieveCall(args);
                    Settings?.Logger?.BeginCall(call.RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.ThrowsException, "ThrowsException_1", args);
                    var result = Void;
                    try
                    {
                        ThrowsException_1(call.RpcEndPoint);
                        Settings?.Logger?.EndCall(call.RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.ThrowsException, "ThrowsException_1", args, result);
                        call.Reply(result);
                    }
                    catch (Exception e) when (!(e is RpcException))
                    {
                        Settings?.Logger?.EndCall(call.RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.ThrowsException, "ThrowsException_1", args, e);
                        call.SystemError();
                        return;
                    }
                    break;
                }
                case TestService2Constants.Echo:
                {
                    var args = new Echo_1_Arguments();
                    call.RetrieveCall(args);
                    Settings?.Logger?.BeginCall(call.RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.Echo, "Echo_1", args);
                    var result = new Echo_1_Result();
                    try
                    {
                        result.Value = Echo_1(call.RpcEndPoint, args.Value);
                        Settings?.Logger?.EndCall(call.RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.Echo, "Echo_1", args, result);
                        call.Reply(result);
                    }
                    catch (Exception e) when (!(e is RpcException))
                    {
                        Settings?.Logger?.EndCall(call.RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.Echo, "Echo_1", args, e);
                        call.SystemError();
                        return;
                    }
                    break;
                }
                case TestService2Constants.NonExistingProcedure:
                {
                    var args = new NonExistingProcedure_1_Arguments();
                    call.RetrieveCall(args);
                    Settings?.Logger?.BeginCall(call.RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.NonExistingProcedure, "NonExistingProcedure_1", args);
                    var result = Void;
                    try
                    {
                        NonExistingProcedure_1(call.RpcEndPoint, args.SomeBytes);
                        Settings?.Logger?.EndCall(call.RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.NonExistingProcedure, "NonExistingProcedure_1", args, result);
                        call.Reply(result);
                    }
                    catch (Exception e) when (!(e is RpcException))
                    {
                        Settings?.Logger?.EndCall(call.RpcEndPoint, TestService2Constants.TestServiceVersion, TestService2Constants.NonExistingProcedure, "NonExistingProcedure_1", args, e);
                        call.SystemError();
                        return;
                    }
                    break;
                }
                default:
                    Settings?.Logger?.Error($"Procedure unavailable (Version: {call.Version}, Procedure: {call.Procedure}).");
                    call.ProcedureUnavailable();
                    break;
            }
        }
        else if (call.Version == TestService2Constants.NonExistingVersion)
        {
            switch (call.Procedure)
            {
                case TestService2Constants.NonExistingProcedure:
                {
                    var args = new NonExistingProcedure_3_Arguments();
                    call.RetrieveCall(args);
                    Settings?.Logger?.BeginCall(call.RpcEndPoint, TestService2Constants.NonExistingVersion, TestService2Constants.NonExistingProcedure, "NonExistingProcedure_3", args);
                    var result = Void;
                    try
                    {
                        NonExistingProcedure_3(call.RpcEndPoint, args.SomeBytes);
                        Settings?.Logger?.EndCall(call.RpcEndPoint, TestService2Constants.NonExistingVersion, TestService2Constants.NonExistingProcedure, "NonExistingProcedure_3", args, result);
                        call.Reply(result);
                    }
                    catch (Exception e) when (!(e is RpcException))
                    {
                        Settings?.Logger?.EndCall(call.RpcEndPoint, TestService2Constants.NonExistingVersion, TestService2Constants.NonExistingProcedure, "NonExistingProcedure_3", args, e);
                        call.SystemError();
                        return;
                    }
                    break;
                }
                default:
                    Settings?.Logger?.Error($"Procedure unavailable (Version: {call.Version}, Procedure: {call.Procedure}).");
                    call.ProcedureUnavailable();
                    break;
            }
        }
        else
        {
            Settings?.Logger?.Error($"Program mismatch (Version: {call.Version}).");
            call.ProgramMismatch();
        }
    }
}
