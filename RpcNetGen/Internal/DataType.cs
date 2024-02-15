// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

internal sealed class DataType
{
    public DataType(RpcParser.DataTypeContext dataType)
    {
        dataType.Check();

        Kind = DataTypeKind.Simple;
        if (dataType.@bool() is not null)
        {
            dataType.@bool().Check();
            Name = "Bool";
            Declaration = "bool";
        }
        else if (dataType.int8() is not null)
        {
            dataType.int8().Check();
            Name = "Int8";
            Declaration = "sbyte";
        }
        else if (dataType.int16() is not null)
        {
            dataType.int16().Check();
            Name = "Int16";
            Declaration = "short";
        }
        else if (dataType.int32() is not null)
        {
            dataType.int32().Check();
            Name = "Int32";
            Declaration = "int";
        }
        else if (dataType.int64() is not null)
        {
            dataType.int64().Check();
            Name = "Int64";
            Declaration = "long";
        }
        else if (dataType.uint8() is not null)
        {
            dataType.uint8().Check();
            Name = "UInt8";
            Declaration = "byte";
        }
        else if (dataType.uint16() is not null)
        {
            dataType.uint16().Check();
            Name = "UInt16";
            Declaration = "ushort";
        }
        else if (dataType.uint32() is not null)
        {
            dataType.uint32().Check();
            Name = "UInt32";
            Declaration = "uint";
        }
        else if (dataType.uint64() is not null)
        {
            dataType.uint64().Check();
            Name = "UInt64";
            Declaration = "ulong";
        }
        else if (dataType.float32() is not null)
        {
            dataType.float32().Check();
            Name = "Float32";
            Declaration = "float";
        }
        else if (dataType.float64() is not null)
        {
            dataType.float64().Check();
            Name = "Float64";
            Declaration = "double";
        }
        else if (dataType.Identifier() is not null)
        {
            Kind = DataTypeKind.Unknown;
            Name = dataType.Identifier().GetText();
            Declaration = Name;
        }
        else
        {
            throw new ParserException("Unknown data type context.");
        }
    }

    private DataType(string name, DataTypeKind kind, string declaration)
    {
        Name = name;
        Kind = kind;
        Declaration = declaration;
    }

    public string Name { get; }
    public DataTypeKind Kind { get; private set; }
    public string Declaration { get; }
    public bool IsString { get; }

    public static DataType CreateOpaque() => new("opaque", DataTypeKind.Opaque, "byte[]");
    public static DataType CreateString() => new("String", DataTypeKind.Simple, "string");
    public static DataType CreateVoid() => new("void", DataTypeKind.Void, "void");

    public void Prepare(Content content)
    {
        if (Kind != DataTypeKind.Unknown)
        {
            return;
        }

        if (content.IsEnum(Name))
        {
            Kind = DataTypeKind.Enum;
        }

        if (content.IsCustomType(Name))
        {
            Kind = DataTypeKind.CustomType;
        }
    }
}
