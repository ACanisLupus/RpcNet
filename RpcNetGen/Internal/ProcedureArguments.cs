// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

internal class ProcedureArguments
{
    private readonly List<Declaration> _arguments = new();
    private readonly string _procedureName;
    private string _structName;
    private bool _isSingleCustomType;
    private Struct _tempParsedStructForClient;
    private Struct _tempParsedStructForServer;

    public ProcedureArguments(RpcParser.ArgumentsContext arguments, string procedureName)
    {
        _procedureName = procedureName;

        if (arguments == null)
        {
            IsSingleVoid = true;
            return;
        }

        arguments.Check();

        if (arguments.@void() != null)
        {
            arguments.@void().Check();
            IsSingleVoid = true;
        }
        else if (arguments.argumentList() != null)
        {
            arguments.argumentList().Check();
            foreach (RpcParser.DeclarationContext declaration in arguments.argumentList().declaration())
            {
                _arguments.Add(new Declaration(declaration, () => false));
            }
        }
        else
        {
            throw new ParserException("Invalid argument.");
        }
    }

    public string VariableName { get; private set; }
    public bool IsSingleVoid { get; }

    public void Prepare(Content content)
    {
        foreach (Declaration argument in _arguments)
        {
            argument.Prepare(content);
        }

        _isSingleCustomType = (_arguments.Count == 1) && (_arguments[0].DataType.Kind == DataTypeKind.CustomType);

        if (!_isSingleCustomType)
        {
            _structName = _procedureName + "_Arguments";
            _tempParsedStructForClient = new Struct
            {
                GenerateConstructors = false,
                Access = "private",
                Name = _structName,
                Partial = false
            };
            _tempParsedStructForServer = new Struct
            {
                GenerateConstructors = false,
                Access = "private",
                Name = _structName,
                Partial = false
            };

            foreach (Declaration argument in _arguments)
            {
                _tempParsedStructForClient.StructItems.Add(argument);
                _tempParsedStructForServer.StructItems.Add(argument);
            }

            VariableName = "args";
        }
        else
        {
            _structName = _arguments[0].DataType.Name;
            VariableName = _arguments[0].NameAsVariable;
        }
    }

    public void DumpStructForClient(XdrFileWriter writer, int indent) =>
        _tempParsedStructForClient?.Dump(writer, indent);

    public void DumpStructForServer(XdrFileWriter writer, int indent) =>
        _tempParsedStructForServer?.Dump(writer, indent);

    public string GetArgumentsForClient()
    {
        if (IsSingleVoid)
        {
            return "";
        }

        return string.Join(", ", _arguments.Select(p => p.DataType.Declaration + " " + p.NameAsVariable));
    }

    public string GetArgumentsForServer()
    {
        if (IsSingleVoid)
        {
            return "";
        }

        if (_isSingleCustomType)
        {
            return VariableName;
        }

        return string.Join(", ", _arguments.Select(p => VariableName + "." + p.NameAsProperty));
    }

    public void DumpClientArgumentsCreation(XdrFileWriter writer, int indent)
    {
        if (_isSingleCustomType)
        {
            return;
        }

        if (IsSingleVoid)
        {
            writer.WriteLine(indent, $"var {VariableName} = Void;");
        }
        else
        {
            writer.WriteLine(indent, $"var {VariableName} = new {_structName}");
            writer.WriteLine(indent, "{");
            foreach (Declaration argument in _arguments)
            {
                writer.WriteLine(indent + 1, $"{argument.NameAsProperty} = {argument.NameAsVariable},");
            }

            writer.WriteLine(indent, "};");
        }
    }

    public void DumpServerArgumentsCreation(XdrFileWriter writer, int indent)
    {
        if (IsSingleVoid)
        {
            writer.WriteLine(indent, $"var {VariableName} = Void;");
        }
        else
        {
            writer.WriteLine(indent, $"var {VariableName} = new {_structName}();");
        }
    }
}
