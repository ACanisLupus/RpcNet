// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

internal class Declaration
{
    private readonly Func<bool> _isLinkedList;

    public Declaration(Settings settings, RpcParser.DeclarationContext declaration, Func<bool> isLinkedList)
    {
        declaration.Check();

        _isLinkedList = isLinkedList;

        if (declaration.opaque() is not null)
        {
            declaration.opaque().Check();
            DataType = DataType.CreateOpaque();
            Identifier = declaration.opaque().Identifier()?.GetText() ?? DataType.Name;
            IsArray = true;
            VariableLength = declaration.opaque().value()?.GetText() ?? "";
        }
        else if (declaration.fixedLengthOpaque() is not null)
        {
            declaration.fixedLengthOpaque().Check();
            DataType = DataType.CreateOpaque();
            Identifier = declaration.fixedLengthOpaque().Identifier()?.GetText() ?? DataType.Name;
            IsArray = true;
            Length = declaration.fixedLengthOpaque().value().GetText();
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
            QuestionMark = "?";
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
            VariableLength = declaration.vector().value()?.GetText() ?? "";
        }
        else
        {
            throw new ParserException("Invalid declaration.");
        }

        if (!string.IsNullOrWhiteSpace(Length) && !int.TryParse(Length, out _))
        {
            Length = settings.ConstantsClassName + '.' + Length;
        }

        if (!string.IsNullOrWhiteSpace(VariableLength) && !int.TryParse(VariableLength, out _))
        {
            VariableLength = settings.ConstantsClassName + '.' + VariableLength;
        }
    }

    public Declaration(DataType dataType, string identifier = "")
    {
        DataType = dataType;
        Identifier = identifier;
    }

    public string Identifier { get; set; }
    public DataType DataType { get; }
    public bool IsLinkedListDeclaration { get; set; }
    public string NameAsProperty => Identifier.ToUpperFirstLetter();
    public string NameAsVariable => Identifier.ToLowerFirstLetter();
    public string QuestionMark { get; } = "";

    private bool IsArray { get; }
    private bool IsVector { get; }
    private bool IsPointer { get; }
    private string Length { get; }
    private string VariableLength { get; }

    public void Prepare(Content content) => DataType.Prepare(content);

    public void DumpItem(XdrFileWriter writer, int indent)
    {
        if (DataType.Kind == DataTypeKind.Void)
        {
            // Nothing to do
        }
        else if (DataType.Kind == DataTypeKind.Opaque)
        {
            if (!string.IsNullOrWhiteSpace(Length))
            {
                writer.WriteLine(indent, $"public byte[] {NameAsProperty} {{ get; }} = new byte[{Length}];");
            }
            else
            {
                writer.WriteLine(indent, $"public byte[] {NameAsProperty} {{ get; set; }} = Array.Empty<byte>();");
            }
        }
        else if (IsVector)
        {
            writer.WriteLine(indent, $"public List<{DataType.Declaration}> {NameAsProperty} {{ get; set; }} = new List<{DataType.Declaration}>({VariableLength});");
        }
        else if (IsArray)
        {
            writer.WriteLine(indent, $"public {DataType.Declaration}[] {NameAsProperty} {{ get; }} = new {DataType.Declaration}[{Length}];");
        }
        else if (IsPointer)
        {
            writer.WriteLine(indent, $"public {DataType.Declaration}? {NameAsProperty} {{ get; set; }}");
        }
        else if (DataType.Kind is DataTypeKind.CustomType or DataTypeKind.Unknown)
        {
            writer.WriteLine(indent, $"public {DataType.Declaration} {NameAsProperty} {{ get; set; }} = new {DataType.Declaration}();");
        }
        else if (DataType.Declaration == "string")
        {
            writer.WriteLine(indent, $"public {DataType.Declaration} {NameAsProperty} {{ get; set; }} = \"\";");
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
            if (!string.IsNullOrWhiteSpace(Length))
            {
                writer.WriteLine(indent, $"writer.WriteFixedLengthOpaque({name});");
            }
            else if (!string.IsNullOrWhiteSpace(VariableLength))
            {
                writer.WriteLine(indent, $"if ({name} is null)");
                writer.WriteLine(indent, "{");
                writer.WriteLine(indent + 1, $"throw new InvalidOperationException(\"{name} must not be null.\");");
                writer.WriteLine(indent, "}");

                writer.WriteLine(indent, $"if ({name}.Length > {VariableLength})");
                writer.WriteLine(indent, "{");
                writer.WriteLine(indent + 1, $"throw new InvalidOperationException(\"{name} must not not have more than {VariableLength} elements.\");");
                writer.WriteLine(indent, "}");

                writer.WriteLine(indent, $"writer.WriteOpaque({name});");
            }
            else
            {
                writer.WriteLine(indent, $"if ({name} is null)");
                writer.WriteLine(indent, "{");
                writer.WriteLine(indent + 1, $"throw new InvalidOperationException(\"{name} must not be null.\");");
                writer.WriteLine(indent, "}");

                writer.WriteLine(indent, $"writer.WriteOpaque({name});");
            }
        }
        else if (IsArray)
        {
            writer.WriteLine(indent, "{");
            writer.WriteLine(indent + 1, $"for (int _idx = 0; _idx < {name}.Length; _idx++)");
            writer.WriteLine(indent + 1, "{");
            WriteWriteStatement(writer, indent + 2, $"{name}[_idx]");
            writer.WriteLine(indent + 1, "}");
            writer.WriteLine(indent, "}");
        }
        else if (IsVector)
        {
            writer.WriteLine(indent, $"if ({name} is not null)");
            writer.WriteLine(indent, "{");

            if (!string.IsNullOrWhiteSpace(VariableLength))
            {
                writer.WriteLine(indent + 1, $"if ({name}.Count > {VariableLength})");
                writer.WriteLine(indent + 1, "{");
                writer.WriteLine(indent + 2, $"throw new InvalidOperationException(\"{name} must not not have more than {VariableLength} elements.\");");
                writer.WriteLine(indent + 1, "}");
            }

            writer.WriteLine(indent + 1, $"int _size = {name}.Count;");
            writer.WriteLine(indent + 1, "writer.Write(_size);");
            writer.WriteLine(indent + 1, "for (int _idx = 0; _idx < _size; _idx++)");
            writer.WriteLine(indent + 1, "{");
            WriteWriteStatement(writer, indent + 2, $"{name}[_idx]");
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
                WriteWriteStatement(writer, indent + 1, name);
                writer.WriteLine(indent, "}");
                writer.WriteLine(indent, "else");
                writer.WriteLine(indent, "{");
                writer.WriteLine(indent + 1, "writer.Write(false);");
                writer.WriteLine(indent, "}");
            }
        }
        else
        {
            WriteWriteStatement(writer, indent, name);
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
            if (!string.IsNullOrWhiteSpace(Length))
            {
                writer.WriteLine(indent, $"reader.ReadFixedLengthOpaque({name});");
            }
            else if (!string.IsNullOrWhiteSpace(VariableLength))
            {
                writer.WriteLine(indent, $"{name} = reader.ReadOpaque();");
                writer.WriteLine(indent, $"if ({name}.Length > {VariableLength})");
                writer.WriteLine(indent, "{");
                writer.WriteLine(indent + 1, $"throw new InvalidOperationException($\"{name} must not not have more than {VariableLength} elements but has {{{name}.Length}}.\");");
                writer.WriteLine(indent, "}");
            }
            else
            {
                writer.WriteLine(indent, $"{name} = reader.ReadOpaque();");
            }
        }
        else if (IsArray)
        {
            writer.WriteLine(indent, "{");
            writer.WriteLine(indent + 1, $"for (int _idx = 0; _idx < {name}.Length; _idx++)");
            writer.WriteLine(indent + 1, "{");
            WriteReadStatement(writer, indent + 2, $"{name}[_idx]");
            writer.WriteLine(indent + 1, "}");
            writer.WriteLine(indent, "}");
        }
        else if (IsVector)
        {
            writer.WriteLine(indent, "{");
            writer.WriteLine(indent + 1, "int _size = reader.ReadInt32();");
            if (!string.IsNullOrWhiteSpace(VariableLength))
            {
                writer.WriteLine(indent + 1, $"if (_size > {VariableLength})");
                writer.WriteLine(indent + 1, "{");
                writer.WriteLine(indent + 2, $"throw new InvalidOperationException($\"{name} must not not have more than {VariableLength} elements but has {{_size}}.\");");
                writer.WriteLine(indent + 1, "}");
            }

            writer.WriteLine(indent + 1, $"{name}.Clear();");
            if (!string.IsNullOrWhiteSpace(VariableLength))
            {
                writer.WriteLine(indent + 1, $"{name}.Capacity = {VariableLength};");
            }

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
            WriteReadStatement(writer, indent, name);
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
            writer.WriteLine(indent, $"if ({name} is null)");
            writer.WriteLine(indent, "{");
            writer.WriteLine(indent + 1, $"sb.Append(\"{prefix}{NameAsProperty} = null\");");
            writer.WriteLine(indent, "}");
            writer.WriteLine(indent, "else");
            writer.WriteLine(indent, "{");
            writer.WriteLine(indent + 1, $"sb.Append(\"{prefix}{NameAsProperty} = [\");");
            writer.WriteLine(indent + 1, $"for (int _idx = 0; _idx < {name}.Length; _idx++)");
            writer.WriteLine(indent + 1, "{");
            writer.WriteLine(indent + 2, "sb.Append(_idx == 0 ? \" \" : \", \");");
            writer.WriteLine(indent + 2, $"sb.Append({name}[_idx]);");
            writer.WriteLine(indent + 1, "}");
            writer.WriteLine(indent + 1, "sb.Append(\" ]\");");
            writer.WriteLine(indent, "}");
        }
        else if (IsArray)
        {
            writer.WriteLine(indent, $"if ({name} is null)");
            writer.WriteLine(indent, "{");
            writer.WriteLine(indent + 1, $"sb.Append(\"{prefix}{NameAsProperty} = null\");");
            writer.WriteLine(indent, "}");
            writer.WriteLine(indent, "else");
            writer.WriteLine(indent, "{");
            writer.WriteLine(indent + 1, $"sb.Append(\"{prefix}{NameAsProperty} = [\");");
            writer.WriteLine(indent + 1, $"for (int _idx = 0; _idx < {name}.Length; _idx++)");
            writer.WriteLine(indent + 1, "{");
            writer.WriteLine(indent + 2, "sb.Append(_idx == 0 ? \" \" : \", \");");
            WriteToStringStatement(writer, indent + 2, $"{name}[_idx]");
            writer.WriteLine(indent + 1, "}");
            writer.WriteLine(indent + 1, "sb.Append(\" ]\");");
            writer.WriteLine(indent, "}");
        }
        else if (IsVector)
        {
            writer.WriteLine(indent, $"if ({name} is null)");
            writer.WriteLine(indent, "{");
            writer.WriteLine(indent + 1, $"sb.Append(\"{prefix}{NameAsProperty} = null\");");
            writer.WriteLine(indent, "}");
            writer.WriteLine(indent, "else");
            writer.WriteLine(indent, "{");
            writer.WriteLine(indent + 1, $"sb.Append(\"{prefix}{NameAsProperty} = [\");");
            writer.WriteLine(indent + 1, $"for (int _idx = 0; _idx < {name}.Count; _idx++)");
            writer.WriteLine(indent + 1, "{");
            writer.WriteLine(indent + 2, "sb.Append(_idx == 0 ? \" \" : \", \");");
            WriteToStringStatement(writer, indent + 2, $"{name}[_idx]");
            writer.WriteLine(indent + 1, "}");
            writer.WriteLine(indent + 1, "sb.Append(\" ]\");");
            writer.WriteLine(indent, "}");
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
                WriteToStringStatement(writer, indent, name);
            }
        }
        else
        {
            writer.WriteLine(indent, $"sb.Append(\"{prefix}{NameAsProperty} = \");");
            WriteToStringStatement(writer, indent, name);
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

    private void WriteToStringStatement(XdrFileWriter writer, int indent, string element)
    {
        if (DataType.Kind is DataTypeKind.Enum or DataTypeKind.Simple)
        {
            writer.WriteLine(indent, $"sb.Append({element});");
            return;
        }

        writer.WriteLine(indent, $"if ({element} is null)");
        writer.WriteLine(indent, "{");
        writer.WriteLine(indent + 1, "sb.Append(\"null\");");
        writer.WriteLine(indent, "}");
        writer.WriteLine(indent, "else");
        writer.WriteLine(indent, "{");
        writer.WriteLine(indent + 1, $"{element}.ToString(sb);");
        writer.WriteLine(indent, "}");
    }

    private void WriteWriteStatement(XdrFileWriter writer, int indent, string element)
    {
        switch (DataType.Kind)
        {
            case DataTypeKind.Simple:
                writer.WriteLine(indent, $"writer.Write({element});");
                break;
            case DataTypeKind.Enum:
                writer.WriteLine(indent, $"writer.Write((int){element});");
                break;
            default:
                writer.WriteLine(indent, $"if ({element} is null)");
                writer.WriteLine(indent, "{");
                writer.WriteLine(indent + 1, $"throw new InvalidOperationException(\"{element} must not be null.\");");
                writer.WriteLine(indent, "}");
                writer.WriteLine(indent, $"{element}.WriteTo(writer);");
                break;
        }
    }

    private void WriteReadStatement(XdrFileWriter writer, int indent, string element)
    {
        if (DataType.Kind is DataTypeKind.Simple or DataTypeKind.Enum)
        {
            writer.WriteLine(indent, $"{element} = {GetReadExpression()};");
            return;
        }

        writer.WriteLine(indent, $"if ({element} is null)");
        writer.WriteLine(indent, "{");
        writer.WriteLine(indent + 1, $"{element} = {GetReadExpression()};");
        writer.WriteLine(indent, "}");
        writer.WriteLine(indent, "else");
        writer.WriteLine(indent, "{");
        writer.WriteLine(indent + 1, $"{element}.ReadFrom(reader);");
        writer.WriteLine(indent, "}");
    }

    private string GetReadExpression() =>
        DataType.Kind switch
        {
            DataTypeKind.Simple => $"reader.Read{DataType.Name}()",
            DataTypeKind.Enum => $"({DataType.Name})reader.ReadInt32()",
            _ => $"new {DataType.Name}(reader)"
        };
}
