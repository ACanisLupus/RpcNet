// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

internal class Declaration
{
    private readonly string _constantsClassName;
    private readonly Func<bool> _isLinkedList;

    public Declaration(string constantsClassName, RpcParser.DeclarationContext declaration, Func<bool> isLinkedList)
    {
        declaration.Check();

        _constantsClassName = constantsClassName;
        _isLinkedList = isLinkedList;

        if (declaration.opaque() is not null)
        {
            declaration.opaque().Check();
            DataType = DataType.CreateOpaque();
            Identifier = declaration.opaque().Identifier()?.GetText() ?? DataType.Name;
            IsArray = true;
        }
        else if (declaration.@string() is not null)
        {
            declaration.@string().Check();
            DataType = DataType.CreateString();
            Identifier = declaration.@string().Identifier()?.GetText() ?? DataType.Name;
        }
        else if (declaration.scalar() is not null)
        {
            declaration.scalar().Check();
            DataType = new DataType(declaration.scalar().dataType());
            Identifier = declaration.scalar().Identifier()?.GetText() ?? DataType.Name;
        }
        else if (declaration.pointer() is not null)
        {
            declaration.pointer().Check();
            DataType = new DataType(declaration.pointer().dataType());
            Identifier = declaration.pointer().Identifier()?.GetText() ?? DataType.Name;
            IsPointer = true;
        }
        else if (declaration.array() is not null)
        {
            declaration.array().Check();
            DataType = new DataType(declaration.array().dataType());
            Identifier = declaration.array().Identifier()?.GetText() ?? DataType.Name;
            IsArray = true;
            Length = declaration.array().value().GetText();
        }
        else if (declaration.vector() is not null)
        {
            declaration.vector().Check();
            DataType = new DataType(declaration.vector().dataType());
            Identifier = declaration.vector().Identifier()?.GetText() ?? DataType.Name;
            IsVector = true;
            Length = declaration.vector().value()?.GetText() ?? "";
        }
        else
        {
            throw new ParserException("Invalid declaration.");
        }
    }

    public Declaration(DataType dataType, string identifier = "")
    {
        DataType = dataType;
        Identifier = identifier;
    }

    public string Identifier { get; set; }
    public DataType DataType { get; }
    public bool IsArray { get; }
    public bool IsVector { get; }
    public bool IsPointer { get; }
    public string Length { get; }

    public bool IsLinkedListDeclaration { get; set; }
    public string NameAsProperty => Identifier.ToUpperFirstLetter();
    public string NameAsVariable => Identifier.ToLowerFirstLetter();

    public void Prepare(Content content) => DataType.Prepare(content);

    public void DumpItem(XdrFileWriter writer, int indent)
    {
        if (DataType.Kind == DataTypeKind.Void)
        {
            // Nothing to do
        }
        else if (DataType.Kind == DataTypeKind.Opaque)
        {
            writer.WriteLine(indent, $"public byte[] {NameAsProperty} {{ get; set; }}");
        }
        else if (IsVector)
        {
            writer.WriteLine(indent, $"public List<{DataType.Declaration}> {NameAsProperty} {{ get; set; }} = new List<{DataType.Declaration}>();");
        }
        else if (IsArray)
        {
            string length = Length;
            if (!int.TryParse(length, out _))
            {
                length = _constantsClassName + '.' + length;
            }

            writer.WriteLine(indent, $"public {DataType.Declaration}[] {NameAsProperty} {{ get; set; }} = new {DataType.Declaration}[{length}];");
        }
        else if (IsPointer)
        {
            writer.WriteLine(indent, $"public {DataType.Declaration} {NameAsProperty} {{ get; set; }}");
        }
        else if (DataType.Kind == DataTypeKind.CustomType)
        {
            writer.WriteLine(indent, $"public {DataType.Declaration} {NameAsProperty} {{ get; set; }} = new {DataType.Declaration}();");
        }
        else
        {
            writer.WriteLine(indent, $"public {DataType.Declaration} {NameAsProperty} {{ get; set; }}");
        }
    }

    public void DumpWrite(XdrFileWriter writer, int indent)
    {
        if (DataType.Kind == DataTypeKind.Void)
        {
            return;
        }

        string name = GetNameForReadAndWrite();

        if (DataType.Kind == DataTypeKind.Opaque)
        {
            writer.WriteLine(indent, $"writer.WriteOpaque({name});");
        }
        else if (IsArray)
        {
            writer.WriteLine(indent, "{");
            writer.WriteLine(indent + 1, $"for (int _idx = 0; _idx < {name}.Length; _idx++)");
            writer.WriteLine(indent + 1, "{");
            writer.WriteLine(indent + 2, GetWriteStatement($"{name}[_idx]"));
            writer.WriteLine(indent + 1, "}");
            writer.WriteLine(indent, "}");
        }
        else if (IsVector)
        {
            writer.WriteLine(indent, $"if ({name} is not null)");
            writer.WriteLine(indent, "{");
            writer.WriteLine(indent + 1, $"int _size = {name}.Count;");
            writer.WriteLine(indent + 1, "writer.Write(_size);");
            writer.WriteLine(indent + 1, "for (int _idx = 0; _idx < _size; _idx++)");
            writer.WriteLine(indent + 1, "{");
            writer.WriteLine(indent + 2, GetWriteStatement($"{name}[_idx]"));
            writer.WriteLine(indent + 1, "}");
            writer.WriteLine(indent, "}");
        }
        else if (IsPointer)
        {
            if (IsLinkedListDeclaration)
            {
                writer.WriteLine(indent, $"current = {name};");
                writer.WriteLine(indent, "writer.Write(current is not null);");
            }
            else
            {
                writer.WriteLine(indent, $"if ({name} is not null)");
                writer.WriteLine(indent, "{");
                writer.WriteLine(indent + 1, "writer.Write(true);");
                writer.WriteLine(indent + 1, GetWriteStatement(name));
                writer.WriteLine(indent, "}");
                writer.WriteLine(indent, "else");
                writer.WriteLine(indent, "{");
                writer.WriteLine(indent + 1, "writer.Write(false);");
                writer.WriteLine(indent, "}");
            }
        }
        else
        {
            writer.WriteLine(indent, GetWriteStatement(name));
        }
    }

    public void DumpRead(XdrFileWriter writer, int indent)
    {
        string name = GetNameForReadAndWrite();

        if (DataType.Kind == DataTypeKind.Void)
        {
            return;
        }

        if (DataType.Kind == DataTypeKind.Opaque)
        {
            writer.WriteLine(indent, $"{name} = reader.ReadOpaque();");
        }
        else if (IsArray)
        {
            writer.WriteLine(indent, "{");
            writer.WriteLine(indent + 1, $"for (int _idx = 0; _idx < {name}.Length; _idx++)");
            writer.WriteLine(indent + 1, "{");
            writer.WriteLine(indent + 2, GetReadStatement($"{name}[_idx]"));
            writer.WriteLine(indent + 1, "}");
            writer.WriteLine(indent, "}");
        }
        else if (IsVector)
        {
            writer.WriteLine(indent, "{");
            writer.WriteLine(indent + 1, "int _size = reader.ReadInt32();");
            writer.WriteLine(indent + 1, $"{name}.Clear();");
            writer.WriteLine(indent + 1, "for (int _idx = 0; _idx < _size; _idx++)");
            writer.WriteLine(indent + 1, "{");
            writer.WriteLine(indent + 2, $"{name}.Add({GetReadExpression()});");
            writer.WriteLine(indent + 1, "}");
            writer.WriteLine(indent, "}");
        }
        else if (IsPointer)
        {
            if (IsLinkedListDeclaration)
            {
                writer.WriteLine(indent, $"next = reader.ReadBool() ? new {DataType.Name}() : null;");
                writer.WriteLine(indent, $"{name} = next;");
                writer.WriteLine(indent, "current = next;");
            }
            else
            {
                writer.WriteLine(indent, $"{name} = reader.ReadBool() ? new {DataType.Name}(reader) : null;");
            }
        }
        else
        {
            writer.WriteLine(indent, GetReadStatement(name));
        }
    }

    public void DumpToString(XdrFileWriter writer, int indent, string prefix)
    {
        if (DataType.Kind == DataTypeKind.Void)
        {
            return;
        }

        string name = GetNameForReadAndWrite();

        if (DataType.Kind == DataTypeKind.Opaque)
        {
            writer.WriteLine(indent, $"sb.Append(\"{prefix}{NameAsProperty} = [\");");
            writer.WriteLine(indent, $"for (int _idx = 0; _idx < {name}.Length; _idx++)");
            writer.WriteLine(indent, "{");
            writer.WriteLine(indent + 1, "sb.Append(_idx == 0 ? \" \" : \", \");");
            writer.WriteLine(indent + 1, $"sb.Append({name}[_idx]);");
            writer.WriteLine(indent, "}");
            writer.WriteLine(indent, "sb.Append(\" ]\");");
        }
        else if (IsArray)
        {
            writer.WriteLine(indent, $"sb.Append(\"{prefix}{NameAsProperty} = [\");");
            writer.WriteLine(indent, $"for (int _idx = 0; _idx < {name}.Length; _idx++)");
            writer.WriteLine(indent, "{");
            writer.WriteLine(indent + 1, "sb.Append(_idx == 0 ? \" \" : \", \");");
            writer.WriteLine(indent + 1, GetToStringStatement($"{name}[_idx]"));
            writer.WriteLine(indent, "}");
            writer.WriteLine(indent, "sb.Append(\" ]\");");
        }
        else if (IsVector)
        {
            writer.WriteLine(indent, $"sb.Append(\"{prefix}{NameAsProperty} = [\");");
            writer.WriteLine(indent, $"for (int _idx = 0; _idx < {name}.Count; _idx++)");
            writer.WriteLine(indent, "{");
            writer.WriteLine(indent + 1, "sb.Append(_idx == 0 ? \" \" : \", \");");
            writer.WriteLine(indent + 1, GetToStringStatement($"{name}[_idx]"));
            writer.WriteLine(indent, "}");
            writer.WriteLine(indent, "sb.Append(\" ]\");");
        }
        else if (IsPointer)
        {
            if (IsLinkedListDeclaration)
            {
                // Not logged on purpose
                writer.WriteLine(indent, $"current = {name};");
            }
            else
            {
                writer.WriteLine(indent, $"sb.Append(\"{prefix}{NameAsProperty} = \");");
                writer.WriteLine(indent, GetToStringNullableStatement(name));
            }
        }
        else
        {
            writer.WriteLine(indent, $"sb.Append(\"{prefix}{NameAsProperty} = \");");
            writer.WriteLine(indent, GetToStringStatement(name));
        }
    }

    private string GetNameForReadAndWrite()
    {
        string name = NameAsProperty;
        if (_isLinkedList?.Invoke() ?? false)
        {
            name = "current." + name;
        }

        return name;
    }

    private string GetToStringStatement(string element) =>
        DataType.Kind switch
        {
            DataTypeKind.Simple => $"sb.Append({element});",
            DataTypeKind.Enum => $"sb.Append({element});",
            _ => $"{element}.ToString(sb);"
        };

    private string GetToStringNullableStatement(string element) =>
        DataType.Kind switch
        {
            DataTypeKind.Simple => $"sb.Append({element});",
            DataTypeKind.Enum => $"sb.Append({element});",
            _ => $"{element}?.ToString(sb);"
        };

    private string GetWriteStatement(string element) =>
        DataType.Kind switch
        {
            DataTypeKind.Simple => $"writer.Write({element});",
            DataTypeKind.Enum => $"writer.Write((int){element});",
            _ => $"{element}.WriteTo(writer);"
        };

    private string GetReadStatement(string element) =>
        DataType.Kind switch
        {
            DataTypeKind.Simple => $"{element} = reader.Read{DataType.Name}();",
            DataTypeKind.Enum => $"{element} = ({DataType.Name})reader.ReadInt32();",
            _ => $"{element}.ReadFrom(reader);"
        };

    private string GetReadExpression() =>
        DataType.Kind switch
        {
            DataTypeKind.Simple => $"reader.Read{DataType.Name}()",
            DataTypeKind.Enum => $"({DataType.Name})reader.ReadInt32()",
            _ => $"new {DataType.Name}(reader)"
        };
}
