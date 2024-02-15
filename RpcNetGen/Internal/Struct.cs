// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

using static RpcParser;

internal class Struct
{
    private readonly bool _isLinkedList;

    public Struct(Settings settings, StructContext @struct, string access)
    {
        Access = access;
        Name = @struct.Identifier().GetText();

        DeclarationContext[] declarations = @struct.declaration();
        for (int i = 0; i < declarations.Length; i++)
        {
            DeclarationContext declaration = declarations[i];
            var parsedDeclaration = new Declaration(settings, declaration, () => _isLinkedList);
            StructItems.Add(parsedDeclaration);

            if ((i == (declarations.Length - 1)) && (Name == parsedDeclaration.DataType.Name))
            {
                _isLinkedList = true;
                parsedDeclaration.IsLinkedListDeclaration = true;
            }
        }
    }

    public Struct(Settings settings, TypedefContext typedef, string access)
    {
        Access = access;
        var parsedDeclaration = new Declaration(settings, typedef.declaration(), () => _isLinkedList);
        StructItems.Add(parsedDeclaration);
        Name = parsedDeclaration.Identifier;
        parsedDeclaration.Identifier = "Value";
    }

    public Struct()
    {
    }

    public string Name { get; set; }
    public string Access { get; set; }
    public bool Partial { get; set; } = true;
    public bool GenerateConstructors { get; set; } = true;
    public List<Declaration> StructItems { get; } = new();

    public void Prepare(Content content)
    {
        foreach (Declaration structItem in StructItems)
        {
            structItem.Prepare(content);
        }
    }

    public void Dump(XdrFileWriter writer, int indent)
    {
        bool containsOnlyOneVoid = ((StructItems.Count == 1) && (StructItems[0].DataType.Kind == DataTypeKind.Void)) || (StructItems.Count == 0);
        if (containsOnlyOneVoid)
        {
            return;
        }

        writer.WriteLine();
        string partial = Partial ? "partial " : "";
        writer.WriteLine(indent, $"{Access} {partial}class {Name} : IXdrDataType");
        writer.WriteLine(indent, "{");

        if (GenerateConstructors)
        {
            writer.WriteLine(indent + 1, $"public {Name}()");
            writer.WriteLine(indent + 1, "{");
            writer.WriteLine(indent + 1, "}");
        }

        if (GenerateConstructors)
        {
            writer.WriteLine();
            writer.WriteLine(indent + 1, $"public {Name}(IXdrReader reader)");
            writer.WriteLine(indent + 1, "{");
            writer.WriteLine(indent + 2, "ReadFrom(reader);");
            writer.WriteLine(indent + 1, "}");
        }

        if (GenerateConstructors)
        {
            writer.WriteLine();
        }

        foreach (Declaration structItem in StructItems)
        {
            structItem.DumpItem(writer, indent + 1);
        }

        writer.WriteLine();

        writer.WriteLine(indent + 1, "public void WriteTo(IXdrWriter writer)");
        writer.WriteLine(indent + 1, "{");
        int nextIndent = indent + 2;
        if (_isLinkedList)
        {
            writer.WriteLine(indent + 2, "var current = this;");
            writer.WriteLine(indent + 2, "do");
            writer.WriteLine(indent + 2, "{");
            nextIndent++;
        }

        foreach (Declaration structItem in StructItems)
        {
            structItem.DumpWrite(writer, nextIndent);
        }

        if (_isLinkedList)
        {
            writer.WriteLine(indent + 2, "} while (current is not null);");
        }

        writer.WriteLine(indent + 1, "}");

        writer.WriteLine();

        writer.WriteLine(indent + 1, "public void ReadFrom(IXdrReader reader)");
        writer.WriteLine(indent + 1, "{");
        nextIndent = indent + 2;
        if (_isLinkedList)
        {
            writer.WriteLine(indent + 2, "var current = this;");
            writer.WriteLine(indent + 2, $"{Name}? next;");
            writer.WriteLine(indent + 2, "do");
            writer.WriteLine(indent + 2, "{");
            nextIndent++;
        }

        foreach (Declaration structItem in StructItems)
        {
            structItem.DumpRead(writer, nextIndent);
        }

        if (_isLinkedList)
        {
            writer.WriteLine(indent + 2, "} while (current is not null);");
        }

        writer.WriteLine(indent + 1, "}");

        writer.WriteLine();
        writer.WriteLine(indent + 1, "public void ToString(StringBuilder sb)");
        writer.WriteLine(indent + 1, "{");

        nextIndent = indent + 2;
        if (_isLinkedList)
        {
            writer.WriteLine(indent + 2, "var current = this;");
            writer.WriteLine(indent + 2, "sb.Append(\"[\");");
            writer.WriteLine(indent + 2, "bool _first = true;");
            writer.WriteLine(indent + 2, "do");
            writer.WriteLine(indent + 2, "{");
            writer.WriteLine(indent + 3, "if (_first)");
            writer.WriteLine(indent + 3, "{");
            writer.WriteLine(indent + 4, "_first = false;");
            writer.WriteLine(indent + 3, "}");
            writer.WriteLine(indent + 3, "else");
            writer.WriteLine(indent + 3, "{");
            writer.WriteLine(indent + 4, "sb.Append(\",\");");
            writer.WriteLine(indent + 3, "}");
            nextIndent++;
        }
        else
        {
            writer.WriteLine(indent + 2, "sb.Append(\"{\");");
        }

        for (int i = 0; i < StructItems.Count; i++)
        {
            Declaration structItem = StructItems[i];
            string prefix = i == 0 ? " " : ", ";
            structItem.DumpToString(writer, nextIndent, prefix);
        }

        if (_isLinkedList)
        {
            writer.WriteLine(indent + 2, "} while (current is not null);");
            writer.WriteLine(indent + 2, "sb.Append(\" ]\");");
        }
        else
        {
            writer.WriteLine(indent + 2, "sb.Append(\" }\");");
        }

        writer.WriteLine(indent + 1, "}");

        writer.WriteLine();
        writer.WriteLine(indent + 1, "public override string ToString()");
        writer.WriteLine(indent + 1, "{");
        writer.WriteLine(indent + 2, "var sb = new StringBuilder();");
        writer.WriteLine(indent + 2, "ToString(sb);");
        writer.WriteLine(indent + 2, "return sb.ToString();");
        writer.WriteLine(indent + 1, "}");

        writer.WriteLine(indent, "}");
    }
}
