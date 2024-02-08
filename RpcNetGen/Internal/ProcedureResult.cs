// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

internal class ProcedureResult
{
    private readonly DataType _dataType;
    private readonly string _procedureName;
    private string _structName;
    private Struct _tempParsedStructForClient;
    private Struct _tempParsedStructForServer;

    public ProcedureResult(string constantsClassName, RpcParser.ReturnContext @return, string procedureName)
    {
        @return.Check();

        _procedureName = procedureName;

        if (@return.declaration() is not null)
        {
            _dataType = new Declaration(constantsClassName, @return.declaration(), () => false).DataType;
        }
        else if (@return.@void() is not null)
        {
            _dataType = DataType.CreateVoid();
        }
    }

    public string Declaration => _dataType.Declaration;
    public string VariableName => "result";

    public void Prepare(Content content)
    {
        _dataType.Prepare(content);
        if (_dataType.Kind == DataTypeKind.Void)
        {
            return;
        }

        if (_dataType.Kind != DataTypeKind.CustomType)
        {
            _structName = _procedureName + "_Result";
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

            var valueDeclaration = new Declaration(_dataType, "Value");
            _tempParsedStructForClient.StructItems.Add(valueDeclaration);
            _tempParsedStructForServer.StructItems.Add(valueDeclaration);
        }
        else
        {
            _structName = _dataType.Name;
        }
    }

    public void DumpStructForClient(XdrFileWriter writer, int indent) => _tempParsedStructForClient?.Dump(writer, indent);
    public void DumpStructForServer(XdrFileWriter writer, int indent) => _tempParsedStructForServer?.Dump(writer, indent);

    public void DumpClientResultCreation(XdrFileWriter writer, int indent)
    {
        if (_dataType.Kind == DataTypeKind.Void)
        {
            writer.WriteLine(indent, $"var {VariableName} = Void;");
        }
        else
        {
            writer.WriteLine(indent, $"var {VariableName} = new {_structName}();");
        }
    }

    public void DumpClientReturn(XdrFileWriter writer, int indent)
    {
        if (_dataType.Kind == DataTypeKind.CustomType)
        {
            writer.WriteLine(indent, $"return {VariableName};");
        }
        else if (_dataType.Kind == DataTypeKind.Void)
        {
            // Nothing to do
        }
        else
        {
            writer.WriteLine(indent, $"return {VariableName}.Value;");
        }
    }

    public void DumpServerResultCreation(XdrFileWriter writer, int indent)
    {
        if (_dataType.Kind == DataTypeKind.Void)
        {
            writer.WriteLine(indent, $"var {VariableName} = Void;");
        }
        else if (_dataType.Kind != DataTypeKind.CustomType)
        {
            writer.WriteLine(indent, $"var {VariableName} = new {_structName}();");
        }
    }

    public string GetServerResult()
    {
        if (_dataType.Kind == DataTypeKind.CustomType)
        {
            return $"{_structName} {VariableName} = ";
        }

        if (_dataType.Kind == DataTypeKind.Void)
        {
            return "";
        }

        return $"{VariableName}.Value = ";
    }
}
