// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

internal class Union
{
    private readonly string _access;
    private readonly List<string> _caseNames = new();
    private readonly Declaration _defaultCase;
    private readonly Declaration _switchDeclaration;
    private readonly List<Declaration> _unionItems = new();

    public Union(string constantsClassName, RpcParser.UnionContext union, string access, Content content)
    {
        _access = access;
        union.Check();

        Name = union.Identifier().GetText();

        RpcParser.DeclarationContext declaration = union.declaration();
        _switchDeclaration = new Declaration(constantsClassName, declaration, () => false);
        if (union.defaultItem() is not null)
        {
            if (union.defaultItem().declaration() is not null)
            {
                _defaultCase = new Declaration(constantsClassName, union.defaultItem().declaration(), () => false);
            }
            else if (union.defaultItem().@void() is not null)
            {
                _defaultCase = new Declaration(DataType.CreateVoid());
            }
        }

        RpcParser.CaseContext[] cases = union.@case();

        foreach (RpcParser.CaseContext @case in cases)
        {
            string caseName = content.GetValue(@case.value());
            caseName = _switchDeclaration.DataType.Name + "." + caseName;
            _caseNames.Add(caseName);
            if (@case.unionItem().declaration() is not null)
            {
                _unionItems.Add(new Declaration(constantsClassName, @case.unionItem().declaration(), () => false));
            }
            else if (@case.unionItem().@void() is not null)
            {
                _unionItems.Add(new Declaration(DataType.CreateVoid()));
            }
        }
    }

    public string Name { get; }
    public IReadOnlyList<Declaration> UnionItems => _unionItems;

    public void Prepare(Content content)
    {
        _switchDeclaration.Prepare(content);
        _defaultCase?.Prepare(content);
        foreach (Declaration unionItem in _unionItems)
        {
            unionItem.Prepare(content);
        }
    }

    public void Dump(XdrFileWriter writer, int indent)
    {
        writer.WriteLine();
        writer.WriteLine(indent, $"{_access} partial class {Name} : IXdrDataType");
        writer.WriteLine(indent, "{");
        writer.WriteLine(indent + 1, $"public {Name}()");
        writer.WriteLine(indent + 1, "{");
        writer.WriteLine(indent + 1, "}");

        writer.WriteLine();
        writer.WriteLine(indent + 1, $"public {Name}(IXdrReader reader)");
        writer.WriteLine(indent + 1, "{");
        writer.WriteLine(indent + 2, "ReadFrom(reader);");
        writer.WriteLine(indent + 1, "}");

        writer.WriteLine();
        _switchDeclaration.DumpItem(writer, indent + 1);
        foreach (Declaration unionItem in UnionItems)
        {
            unionItem.DumpItem(writer, indent + 1);
        }

        writer.WriteLine();
        writer.WriteLine(indent + 1, "public void WriteTo(IXdrWriter writer)");
        writer.WriteLine(indent + 1, "{");
        _switchDeclaration.DumpWrite(writer, indent + 2);
        writer.WriteLine(indent + 2, $"switch ({_switchDeclaration.NameAsProperty})");
        writer.WriteLine(indent + 2, "{");
        for (int i = 0; i < _caseNames.Count; i++)
        {
            writer.WriteLine(indent + 3, $"case {_caseNames[i]}:");
            _unionItems[i].DumpWrite(writer, indent + 4);
            writer.WriteLine(indent + 4, "break;");
        }

        if (_defaultCase is not null)
        {
            writer.WriteLine(indent + 3, "default:");
            _defaultCase.DumpWrite(writer, indent + 4);
            writer.WriteLine(indent + 4, "break;");
        }

        writer.WriteLine(indent + 2, "}");
        writer.WriteLine(indent + 1, "}");

        writer.WriteLine();
        writer.WriteLine(indent + 1, "public void ReadFrom(IXdrReader reader)");
        writer.WriteLine(indent + 1, "{");
        _switchDeclaration.DumpRead(writer, indent + 2);
        writer.WriteLine(indent + 2, $"switch ({_switchDeclaration.NameAsProperty})");
        writer.WriteLine(indent + 2, "{");
        for (int i = 0; i < _caseNames.Count; i++)
        {
            writer.WriteLine(indent + 3, $"case {_caseNames[i]}:");
            _unionItems[i].DumpRead(writer, indent + 4);
            writer.WriteLine(indent + 4, "break;");
        }

        if (_defaultCase is not null)
        {
            writer.WriteLine(indent + 3, "default:");
            _defaultCase.DumpRead(writer, indent + 4);
            writer.WriteLine(indent + 4, "break;");
        }

        writer.WriteLine(indent + 2, "}");
        writer.WriteLine(indent + 1, "}");

        writer.WriteLine();
        writer.WriteLine(indent + 1, "public void ToString(StringBuilder sb)");
        writer.WriteLine(indent + 1, "{");
        writer.WriteLine(indent + 2, "sb.Append(\"{\");");
        writer.WriteLine(indent + 2, $"switch ({_switchDeclaration.NameAsProperty})");
        writer.WriteLine(indent + 2, "{");
        for (int i = 0; i < _caseNames.Count; i++)
        {
            writer.WriteLine(indent + 3, $"case {_caseNames[i]}:");
            _unionItems[i].DumpToString(writer, indent + 4, " ");
            writer.WriteLine(indent + 4, "break;");
        }

        if (_defaultCase is not null)
        {
            writer.WriteLine(indent + 3, "default:");
            _defaultCase.DumpToString(writer, indent + 4, " ");
            writer.WriteLine(indent + 4, "break;");
        }

        writer.WriteLine(indent + 2, "}");
        writer.WriteLine(indent + 2, "sb.Append(\" }\");");
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
