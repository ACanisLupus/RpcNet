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

internal static class TestServiceConstants
{
    public const int Echo = 2;
    public const int SimpleStructSimpleStruct = 3;
    public const int TestServiceProgram = 0x20406080;
    public const int TestServiceVersion = 1;
    public const int TestServiceVersion2 = 2;
    public const int ThrowsException = 1;
}

internal enum SimpleEnum
{
    Value1,
    Value2 = 1,
}

internal partial class ComplexStruct : IXdrDataType
{
    public ComplexStruct()
    {
    }

    public ComplexStruct(IXdrReader reader)
    {
        ReadFrom(reader);
    }

    public bool BoolValue { get; set; }
    public sbyte Int8Value { get; set; }
    public short Int16Value { get; set; }
    public int Int32Value { get; set; }
    public long Int64Value { get; set; }
    public byte UInt8Value { get; set; }
    public ushort UInt16Value { get; set; }
    public uint UInt32Value { get; set; }
    public ulong UInt64Value { get; set; }
    public float Float32Value { get; set; }
    public double Float64Value { get; set; }
    public SimpleStruct SimpleStructValue { get; set; } = new SimpleStruct();
    public SimpleEnum SimpleEnumValue { get; set; }
    public byte[] DynamicOpaque { get; set; } = Array.Empty<byte>();
    public byte[] DynamicLimitedOpaque { get; set; } = Array.Empty<byte>();
    public byte[] FixedLengthOpaque { get; } = new byte[10];
    public List<byte> DynamicUInt8Array { get; set; } = new List<byte>();
    public List<byte> DynamicLimitedUInt8Array { get; set; } = new List<byte>(10);
    public byte[] FixedLengthUInt8Array { get; } = new byte[10];
    public List<SimpleStruct> DynamicSimpleStructArray { get; set; } = new List<SimpleStruct>();
    public List<SimpleStruct> DynamicLimitedSimpleStructArray { get; set; } = new List<SimpleStruct>(10);
    public SimpleStruct[] FixedLengthSimpleStructArray { get; } = new SimpleStruct[10];
    public List<SimpleEnum> DynamicSimpleEnumArray { get; set; } = new List<SimpleEnum>();
    public List<SimpleEnum> DynamicLimitedSimpleEnumArray { get; set; } = new List<SimpleEnum>(10);
    public SimpleEnum[] FixedLengthSimpleEnumArray { get; } = new SimpleEnum[10];
    public StringType[] StringArray { get; } = new StringType[10];

    public void WriteTo(IXdrWriter writer)
    {
        writer.Write(BoolValue);
        writer.Write(Int8Value);
        writer.Write(Int16Value);
        writer.Write(Int32Value);
        writer.Write(Int64Value);
        writer.Write(UInt8Value);
        writer.Write(UInt16Value);
        writer.Write(UInt32Value);
        writer.Write(UInt64Value);
        writer.Write(Float32Value);
        writer.Write(Float64Value);
        if (SimpleStructValue is null)
        {
            throw new InvalidOperationException("SimpleStructValue must not be null.");
        }
        SimpleStructValue.WriteTo(writer);
        writer.Write((int)SimpleEnumValue);
        if (DynamicOpaque is null)
        {
            throw new InvalidOperationException("DynamicOpaque must not be null.");
        }
        writer.WriteOpaque(DynamicOpaque);
        if (DynamicLimitedOpaque is null)
        {
            throw new InvalidOperationException("DynamicLimitedOpaque must not be null.");
        }
        if (DynamicLimitedOpaque.Length > 10)
        {
            throw new InvalidOperationException("DynamicLimitedOpaque must not not have more than 10 elements.");
        }
        writer.WriteOpaque(DynamicLimitedOpaque);
        writer.WriteFixedLengthOpaque(FixedLengthOpaque);
        if (DynamicUInt8Array is not null)
        {
            int _size = DynamicUInt8Array.Count;
            writer.Write(_size);
            for (int _idx = 0; _idx < _size; _idx++)
            {
                writer.Write(DynamicUInt8Array[_idx]);
            }
        }
        if (DynamicLimitedUInt8Array is not null)
        {
            if (DynamicLimitedUInt8Array.Count > 10)
            {
                throw new InvalidOperationException("DynamicLimitedUInt8Array must not not have more than 10 elements.");
            }
            int _size = DynamicLimitedUInt8Array.Count;
            writer.Write(_size);
            for (int _idx = 0; _idx < _size; _idx++)
            {
                writer.Write(DynamicLimitedUInt8Array[_idx]);
            }
        }
        {
            for (int _idx = 0; _idx < FixedLengthUInt8Array.Length; _idx++)
            {
                writer.Write(FixedLengthUInt8Array[_idx]);
            }
        }
        if (DynamicSimpleStructArray is not null)
        {
            int _size = DynamicSimpleStructArray.Count;
            writer.Write(_size);
            for (int _idx = 0; _idx < _size; _idx++)
            {
                if (DynamicSimpleStructArray[_idx] is null)
                {
                    throw new InvalidOperationException("DynamicSimpleStructArray[_idx] must not be null.");
                }
                DynamicSimpleStructArray[_idx].WriteTo(writer);
            }
        }
        if (DynamicLimitedSimpleStructArray is not null)
        {
            if (DynamicLimitedSimpleStructArray.Count > 10)
            {
                throw new InvalidOperationException("DynamicLimitedSimpleStructArray must not not have more than 10 elements.");
            }
            int _size = DynamicLimitedSimpleStructArray.Count;
            writer.Write(_size);
            for (int _idx = 0; _idx < _size; _idx++)
            {
                if (DynamicLimitedSimpleStructArray[_idx] is null)
                {
                    throw new InvalidOperationException("DynamicLimitedSimpleStructArray[_idx] must not be null.");
                }
                DynamicLimitedSimpleStructArray[_idx].WriteTo(writer);
            }
        }
        {
            for (int _idx = 0; _idx < FixedLengthSimpleStructArray.Length; _idx++)
            {
                if (FixedLengthSimpleStructArray[_idx] is null)
                {
                    throw new InvalidOperationException("FixedLengthSimpleStructArray[_idx] must not be null.");
                }
                FixedLengthSimpleStructArray[_idx].WriteTo(writer);
            }
        }
        if (DynamicSimpleEnumArray is not null)
        {
            int _size = DynamicSimpleEnumArray.Count;
            writer.Write(_size);
            for (int _idx = 0; _idx < _size; _idx++)
            {
                writer.Write((int)DynamicSimpleEnumArray[_idx]);
            }
        }
        if (DynamicLimitedSimpleEnumArray is not null)
        {
            if (DynamicLimitedSimpleEnumArray.Count > 10)
            {
                throw new InvalidOperationException("DynamicLimitedSimpleEnumArray must not not have more than 10 elements.");
            }
            int _size = DynamicLimitedSimpleEnumArray.Count;
            writer.Write(_size);
            for (int _idx = 0; _idx < _size; _idx++)
            {
                writer.Write((int)DynamicLimitedSimpleEnumArray[_idx]);
            }
        }
        {
            for (int _idx = 0; _idx < FixedLengthSimpleEnumArray.Length; _idx++)
            {
                writer.Write((int)FixedLengthSimpleEnumArray[_idx]);
            }
        }
        {
            for (int _idx = 0; _idx < StringArray.Length; _idx++)
            {
                if (StringArray[_idx] is null)
                {
                    throw new InvalidOperationException("StringArray[_idx] must not be null.");
                }
                StringArray[_idx].WriteTo(writer);
            }
        }
    }

    public void ReadFrom(IXdrReader reader)
    {
        BoolValue = reader.ReadBool();
        Int8Value = reader.ReadInt8();
        Int16Value = reader.ReadInt16();
        Int32Value = reader.ReadInt32();
        Int64Value = reader.ReadInt64();
        UInt8Value = reader.ReadUInt8();
        UInt16Value = reader.ReadUInt16();
        UInt32Value = reader.ReadUInt32();
        UInt64Value = reader.ReadUInt64();
        Float32Value = reader.ReadFloat32();
        Float64Value = reader.ReadFloat64();
        if (SimpleStructValue is null)
        {
            SimpleStructValue = new SimpleStruct(reader);
        }
        else
        {
            SimpleStructValue.ReadFrom(reader);
        }
        SimpleEnumValue = (SimpleEnum)reader.ReadInt32();
        DynamicOpaque = reader.ReadOpaque();
        DynamicLimitedOpaque = reader.ReadOpaque();
        if (DynamicLimitedOpaque.Length > 10)
        {
            throw new InvalidOperationException($"DynamicLimitedOpaque must not not have more than 10 elements but has {DynamicLimitedOpaque.Length}.");
        }
        reader.ReadFixedLengthOpaque(FixedLengthOpaque);
        {
            int _size = reader.ReadInt32();
            DynamicUInt8Array.Clear();
            for (int _idx = 0; _idx < _size; _idx++)
            {
                DynamicUInt8Array.Add(reader.ReadUInt8());
            }
        }
        {
            int _size = reader.ReadInt32();
            if (_size > 10)
            {
                throw new InvalidOperationException($"DynamicLimitedUInt8Array must not not have more than 10 elements but has {_size}.");
            }
            DynamicLimitedUInt8Array.Clear();
            DynamicLimitedUInt8Array.Capacity = 10;
            for (int _idx = 0; _idx < _size; _idx++)
            {
                DynamicLimitedUInt8Array.Add(reader.ReadUInt8());
            }
        }
        {
            for (int _idx = 0; _idx < FixedLengthUInt8Array.Length; _idx++)
            {
                FixedLengthUInt8Array[_idx] = reader.ReadUInt8();
            }
        }
        {
            int _size = reader.ReadInt32();
            DynamicSimpleStructArray.Clear();
            for (int _idx = 0; _idx < _size; _idx++)
            {
                DynamicSimpleStructArray.Add(new SimpleStruct(reader));
            }
        }
        {
            int _size = reader.ReadInt32();
            if (_size > 10)
            {
                throw new InvalidOperationException($"DynamicLimitedSimpleStructArray must not not have more than 10 elements but has {_size}.");
            }
            DynamicLimitedSimpleStructArray.Clear();
            DynamicLimitedSimpleStructArray.Capacity = 10;
            for (int _idx = 0; _idx < _size; _idx++)
            {
                DynamicLimitedSimpleStructArray.Add(new SimpleStruct(reader));
            }
        }
        {
            for (int _idx = 0; _idx < FixedLengthSimpleStructArray.Length; _idx++)
            {
                if (FixedLengthSimpleStructArray[_idx] is null)
                {
                    FixedLengthSimpleStructArray[_idx] = new SimpleStruct(reader);
                }
                else
                {
                    FixedLengthSimpleStructArray[_idx].ReadFrom(reader);
                }
            }
        }
        {
            int _size = reader.ReadInt32();
            DynamicSimpleEnumArray.Clear();
            for (int _idx = 0; _idx < _size; _idx++)
            {
                DynamicSimpleEnumArray.Add((SimpleEnum)reader.ReadInt32());
            }
        }
        {
            int _size = reader.ReadInt32();
            if (_size > 10)
            {
                throw new InvalidOperationException($"DynamicLimitedSimpleEnumArray must not not have more than 10 elements but has {_size}.");
            }
            DynamicLimitedSimpleEnumArray.Clear();
            DynamicLimitedSimpleEnumArray.Capacity = 10;
            for (int _idx = 0; _idx < _size; _idx++)
            {
                DynamicLimitedSimpleEnumArray.Add((SimpleEnum)reader.ReadInt32());
            }
        }
        {
            for (int _idx = 0; _idx < FixedLengthSimpleEnumArray.Length; _idx++)
            {
                FixedLengthSimpleEnumArray[_idx] = (SimpleEnum)reader.ReadInt32();
            }
        }
        {
            for (int _idx = 0; _idx < StringArray.Length; _idx++)
            {
                if (StringArray[_idx] is null)
                {
                    StringArray[_idx] = new StringType(reader);
                }
                else
                {
                    StringArray[_idx].ReadFrom(reader);
                }
            }
        }
    }

    public void ToString(StringBuilder sb)
    {
        sb.Append("{");
        sb.Append(" BoolValue = ");
        sb.Append(BoolValue);
        sb.Append(", Int8Value = ");
        sb.Append(Int8Value);
        sb.Append(", Int16Value = ");
        sb.Append(Int16Value);
        sb.Append(", Int32Value = ");
        sb.Append(Int32Value);
        sb.Append(", Int64Value = ");
        sb.Append(Int64Value);
        sb.Append(", UInt8Value = ");
        sb.Append(UInt8Value);
        sb.Append(", UInt16Value = ");
        sb.Append(UInt16Value);
        sb.Append(", UInt32Value = ");
        sb.Append(UInt32Value);
        sb.Append(", UInt64Value = ");
        sb.Append(UInt64Value);
        sb.Append(", Float32Value = ");
        sb.Append(Float32Value);
        sb.Append(", Float64Value = ");
        sb.Append(Float64Value);
        sb.Append(", SimpleStructValue = ");
        if (SimpleStructValue is null)
        {
            sb.Append("null");
        }
        else
        {
            SimpleStructValue.ToString(sb);
        }
        sb.Append(", SimpleEnumValue = ");
        sb.Append(SimpleEnumValue);
        if (DynamicOpaque is null)
        {
            sb.Append(", DynamicOpaque = null");
        }
        else
        {
            sb.Append(", DynamicOpaque = [");
            for (int _idx = 0; _idx < DynamicOpaque.Length; _idx++)
            {
                sb.Append(_idx == 0 ? " " : ", ");
                sb.Append(DynamicOpaque[_idx]);
            }
            sb.Append(" ]");
        }
        if (DynamicLimitedOpaque is null)
        {
            sb.Append(", DynamicLimitedOpaque = null");
        }
        else
        {
            sb.Append(", DynamicLimitedOpaque = [");
            for (int _idx = 0; _idx < DynamicLimitedOpaque.Length; _idx++)
            {
                sb.Append(_idx == 0 ? " " : ", ");
                sb.Append(DynamicLimitedOpaque[_idx]);
            }
            sb.Append(" ]");
        }
        if (FixedLengthOpaque is null)
        {
            sb.Append(", FixedLengthOpaque = null");
        }
        else
        {
            sb.Append(", FixedLengthOpaque = [");
            for (int _idx = 0; _idx < FixedLengthOpaque.Length; _idx++)
            {
                sb.Append(_idx == 0 ? " " : ", ");
                sb.Append(FixedLengthOpaque[_idx]);
            }
            sb.Append(" ]");
        }
        if (DynamicUInt8Array is null)
        {
            sb.Append(", DynamicUInt8Array = null");
        }
        else
        {
            sb.Append(", DynamicUInt8Array = [");
            for (int _idx = 0; _idx < DynamicUInt8Array.Count; _idx++)
            {
                sb.Append(_idx == 0 ? " " : ", ");
                sb.Append(DynamicUInt8Array[_idx]);
            }
            sb.Append(" ]");
        }
        if (DynamicLimitedUInt8Array is null)
        {
            sb.Append(", DynamicLimitedUInt8Array = null");
        }
        else
        {
            sb.Append(", DynamicLimitedUInt8Array = [");
            for (int _idx = 0; _idx < DynamicLimitedUInt8Array.Count; _idx++)
            {
                sb.Append(_idx == 0 ? " " : ", ");
                sb.Append(DynamicLimitedUInt8Array[_idx]);
            }
            sb.Append(" ]");
        }
        if (FixedLengthUInt8Array is null)
        {
            sb.Append(", FixedLengthUInt8Array = null");
        }
        else
        {
            sb.Append(", FixedLengthUInt8Array = [");
            for (int _idx = 0; _idx < FixedLengthUInt8Array.Length; _idx++)
            {
                sb.Append(_idx == 0 ? " " : ", ");
                sb.Append(FixedLengthUInt8Array[_idx]);
            }
            sb.Append(" ]");
        }
        if (DynamicSimpleStructArray is null)
        {
            sb.Append(", DynamicSimpleStructArray = null");
        }
        else
        {
            sb.Append(", DynamicSimpleStructArray = [");
            for (int _idx = 0; _idx < DynamicSimpleStructArray.Count; _idx++)
            {
                sb.Append(_idx == 0 ? " " : ", ");
                if (DynamicSimpleStructArray[_idx] is null)
                {
                    sb.Append("null");
                }
                else
                {
                    DynamicSimpleStructArray[_idx].ToString(sb);
                }
            }
            sb.Append(" ]");
        }
        if (DynamicLimitedSimpleStructArray is null)
        {
            sb.Append(", DynamicLimitedSimpleStructArray = null");
        }
        else
        {
            sb.Append(", DynamicLimitedSimpleStructArray = [");
            for (int _idx = 0; _idx < DynamicLimitedSimpleStructArray.Count; _idx++)
            {
                sb.Append(_idx == 0 ? " " : ", ");
                if (DynamicLimitedSimpleStructArray[_idx] is null)
                {
                    sb.Append("null");
                }
                else
                {
                    DynamicLimitedSimpleStructArray[_idx].ToString(sb);
                }
            }
            sb.Append(" ]");
        }
        if (FixedLengthSimpleStructArray is null)
        {
            sb.Append(", FixedLengthSimpleStructArray = null");
        }
        else
        {
            sb.Append(", FixedLengthSimpleStructArray = [");
            for (int _idx = 0; _idx < FixedLengthSimpleStructArray.Length; _idx++)
            {
                sb.Append(_idx == 0 ? " " : ", ");
                if (FixedLengthSimpleStructArray[_idx] is null)
                {
                    sb.Append("null");
                }
                else
                {
                    FixedLengthSimpleStructArray[_idx].ToString(sb);
                }
            }
            sb.Append(" ]");
        }
        if (DynamicSimpleEnumArray is null)
        {
            sb.Append(", DynamicSimpleEnumArray = null");
        }
        else
        {
            sb.Append(", DynamicSimpleEnumArray = [");
            for (int _idx = 0; _idx < DynamicSimpleEnumArray.Count; _idx++)
            {
                sb.Append(_idx == 0 ? " " : ", ");
                sb.Append(DynamicSimpleEnumArray[_idx]);
            }
            sb.Append(" ]");
        }
        if (DynamicLimitedSimpleEnumArray is null)
        {
            sb.Append(", DynamicLimitedSimpleEnumArray = null");
        }
        else
        {
            sb.Append(", DynamicLimitedSimpleEnumArray = [");
            for (int _idx = 0; _idx < DynamicLimitedSimpleEnumArray.Count; _idx++)
            {
                sb.Append(_idx == 0 ? " " : ", ");
                sb.Append(DynamicLimitedSimpleEnumArray[_idx]);
            }
            sb.Append(" ]");
        }
        if (FixedLengthSimpleEnumArray is null)
        {
            sb.Append(", FixedLengthSimpleEnumArray = null");
        }
        else
        {
            sb.Append(", FixedLengthSimpleEnumArray = [");
            for (int _idx = 0; _idx < FixedLengthSimpleEnumArray.Length; _idx++)
            {
                sb.Append(_idx == 0 ? " " : ", ");
                sb.Append(FixedLengthSimpleEnumArray[_idx]);
            }
            sb.Append(" ]");
        }
        if (StringArray is null)
        {
            sb.Append(", StringArray = null");
        }
        else
        {
            sb.Append(", StringArray = [");
            for (int _idx = 0; _idx < StringArray.Length; _idx++)
            {
                sb.Append(_idx == 0 ? " " : ", ");
                if (StringArray[_idx] is null)
                {
                    sb.Append("null");
                }
                else
                {
                    StringArray[_idx].ToString(sb);
                }
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

internal partial class SimpleStruct : IXdrDataType
{
    public SimpleStruct()
    {
    }

    public SimpleStruct(IXdrReader reader)
    {
        ReadFrom(reader);
    }

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

internal partial class StringType : IXdrDataType
{
    public StringType()
    {
    }

    public StringType(IXdrReader reader)
    {
        ReadFrom(reader);
    }

    public string Value { get; set; } = "";

    public void WriteTo(IXdrWriter writer)
    {
        writer.Write(Value);
    }

    public void ReadFrom(IXdrReader reader)
    {
        Value = reader.ReadString();
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

internal class TestServiceClient : ClientStub
{
    public TestServiceClient(Protocol protocol, IPAddress ipAddress, int port = 0, ClientSettings? clientSettings = default) :
        base(protocol, ipAddress, port, TestServiceConstants.TestServiceProgram, TestServiceConstants.TestServiceVersion2, clientSettings)
    {
    }

    public void ThrowsException_1()
    {
        var args = Void;
        var result = Void;
        Settings?.Logger?.BeginCall(RpcEndPoint, TestServiceConstants.TestServiceVersion, TestServiceConstants.ThrowsException, "ThrowsException_1", args);
        try
        {
            Call(TestServiceConstants.ThrowsException, TestServiceConstants.TestServiceVersion, args, result);
        }
        catch (Exception e)
        {
            Settings?.Logger?.EndCall(RpcEndPoint, TestServiceConstants.TestServiceVersion, TestServiceConstants.ThrowsException, "ThrowsException_1", args, e);
            throw;
        }
        Settings?.Logger?.EndCall(RpcEndPoint, TestServiceConstants.TestServiceVersion, TestServiceConstants.ThrowsException, "ThrowsException_1", args, result);
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
        Settings?.Logger?.BeginCall(RpcEndPoint, TestServiceConstants.TestServiceVersion, TestServiceConstants.Echo, "Echo_1", args);
        try
        {
            Call(TestServiceConstants.Echo, TestServiceConstants.TestServiceVersion, args, result);
        }
        catch (Exception e)
        {
            Settings?.Logger?.EndCall(RpcEndPoint, TestServiceConstants.TestServiceVersion, TestServiceConstants.Echo, "Echo_1", args, e);
            throw;
        }
        Settings?.Logger?.EndCall(RpcEndPoint, TestServiceConstants.TestServiceVersion, TestServiceConstants.Echo, "Echo_1", args, result);
        return result.Value;
    }

    public SimpleStruct SimpleStructSimpleStruct_2(SimpleStruct value)
    {
        var result = new SimpleStruct();
        Settings?.Logger?.BeginCall(RpcEndPoint, TestServiceConstants.TestServiceVersion2, TestServiceConstants.SimpleStructSimpleStruct, "SimpleStructSimpleStruct_2", value);
        try
        {
            Call(TestServiceConstants.SimpleStructSimpleStruct, TestServiceConstants.TestServiceVersion2, value, result);
        }
        catch (Exception e)
        {
            Settings?.Logger?.EndCall(RpcEndPoint, TestServiceConstants.TestServiceVersion2, TestServiceConstants.SimpleStructSimpleStruct, "SimpleStructSimpleStruct_2", value, e);
            throw;
        }
        Settings?.Logger?.EndCall(RpcEndPoint, TestServiceConstants.TestServiceVersion2, TestServiceConstants.SimpleStructSimpleStruct, "SimpleStructSimpleStruct_2", value, result);
        return result;
    }
}

internal abstract class TestServiceServerStub : ServerStub
{
    public TestServiceServerStub(Protocol protocol, IPAddress ipAddress, int port = 0, ServerSettings? serverSettings = default) :
        base(protocol, ipAddress, port, TestServiceConstants.TestServiceProgram, new[] { TestServiceConstants.TestServiceVersion, TestServiceConstants.TestServiceVersion2 }, serverSettings)
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

    public abstract void ThrowsException_1(RpcEndPoint rpcEndPoint);
    public abstract int Echo_1(RpcEndPoint rpcEndPoint, int value);
    public abstract SimpleStruct SimpleStructSimpleStruct_2(RpcEndPoint rpcEndPoint, SimpleStruct value);

    protected override void DispatchReceivedCall(ReceivedRpcCall call)
    {
        if (call.Version == TestServiceConstants.TestServiceVersion)
        {
            switch (call.Procedure)
            {
                case TestServiceConstants.ThrowsException:
                {
                    var args = Void;
                    call.RetrieveCall(args);
                    Settings?.Logger?.BeginCall(call.RpcEndPoint, TestServiceConstants.TestServiceVersion, TestServiceConstants.ThrowsException, "ThrowsException_1", args);
                    var result = Void;
                    try
                    {
                        ThrowsException_1(call.RpcEndPoint);
                        Settings?.Logger?.EndCall(call.RpcEndPoint, TestServiceConstants.TestServiceVersion, TestServiceConstants.ThrowsException, "ThrowsException_1", args, result);
                        call.Reply(result);
                    }
                    catch (Exception e) when (!(e is RpcException))
                    {
                        Settings?.Logger?.EndCall(call.RpcEndPoint, TestServiceConstants.TestServiceVersion, TestServiceConstants.ThrowsException, "ThrowsException_1", args, e);
                        call.SystemError();
                        return;
                    }
                    break;
                }
                case TestServiceConstants.Echo:
                {
                    var args = new Echo_1_Arguments();
                    call.RetrieveCall(args);
                    Settings?.Logger?.BeginCall(call.RpcEndPoint, TestServiceConstants.TestServiceVersion, TestServiceConstants.Echo, "Echo_1", args);
                    var result = new Echo_1_Result();
                    try
                    {
                        result.Value = Echo_1(call.RpcEndPoint, args.Value);
                        Settings?.Logger?.EndCall(call.RpcEndPoint, TestServiceConstants.TestServiceVersion, TestServiceConstants.Echo, "Echo_1", args, result);
                        call.Reply(result);
                    }
                    catch (Exception e) when (!(e is RpcException))
                    {
                        Settings?.Logger?.EndCall(call.RpcEndPoint, TestServiceConstants.TestServiceVersion, TestServiceConstants.Echo, "Echo_1", args, e);
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
        else if (call.Version == TestServiceConstants.TestServiceVersion2)
        {
            switch (call.Procedure)
            {
                case TestServiceConstants.SimpleStructSimpleStruct:
                {
                    var value = new SimpleStruct();
                    call.RetrieveCall(value);
                    Settings?.Logger?.BeginCall(call.RpcEndPoint, TestServiceConstants.TestServiceVersion2, TestServiceConstants.SimpleStructSimpleStruct, "SimpleStructSimpleStruct_2", value);
                    try
                    {
                        SimpleStruct result = SimpleStructSimpleStruct_2(call.RpcEndPoint, value);
                        Settings?.Logger?.EndCall(call.RpcEndPoint, TestServiceConstants.TestServiceVersion2, TestServiceConstants.SimpleStructSimpleStruct, "SimpleStructSimpleStruct_2", value, result);
                        call.Reply(result);
                    }
                    catch (Exception e) when (!(e is RpcException))
                    {
                        Settings?.Logger?.EndCall(call.RpcEndPoint, TestServiceConstants.TestServiceVersion2, TestServiceConstants.SimpleStructSimpleStruct, "SimpleStructSimpleStruct_2", value, e);
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
