// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

internal class Procedure
{
    private readonly string _argumentConstant;
    private readonly ProcedureResult _procedureResult;
    private readonly ProcedureArguments _procedureArguments;
    private readonly string _versionConstant;

    public Procedure(
        string version,
        string versionConstant,
        RpcParser.ProcedureContext procedureContext,
        Content content)
    {
        _versionConstant = versionConstant;
        Name = procedureContext.Identifier().GetText();
        FullName = Name + "_" + version;
        _argumentConstant = content.AddConstant(Name, content.GetConstant(procedureContext.constant()));

        _procedureArguments = new ProcedureArguments(procedureContext.arguments(), FullName);
        _procedureResult = new ProcedureResult(procedureContext.@return(), FullName);
    }

    public string Name { get; }
    public string FullName { get; }

    public void Prepare(Content content)
    {
        _procedureArguments.Prepare(content);
        _procedureResult.Prepare(content);
    }

    public void DumpForClient(XdrFileWriter writer, int indent)
    {
        _procedureArguments.DumpStructForClient(writer, indent);
        _procedureResult.DumpStructForClient(writer, indent);

        writer.WriteLine();
        writer.WriteLine(
            indent,
            $"public {_procedureResult.Declaration} {FullName}({_procedureArguments.GetArgumentsForClient()})");
        writer.WriteLine(indent, "{");
        _procedureArguments.DumpClientArgumentsCreation(writer, indent + 1);
        _procedureResult.DumpClientResultCreation(writer, indent + 1);
        writer.WriteLine(
            indent + 1,
            $"Settings?.Logger?.BeginCall({_versionConstant}, {_argumentConstant}, \"{Name}\", {_procedureArguments.VariableName});");
        writer.WriteLine(
            indent + 1,
            $"Call({_argumentConstant}, {_versionConstant}, {_procedureArguments.VariableName}, {_procedureResult.VariableName});");
        writer.WriteLine(
            indent + 1,
            $"Settings?.Logger?.EndCall({_versionConstant}, {_argumentConstant}, \"{Name}\", {_procedureArguments.VariableName}, {_procedureResult.VariableName});");
        _procedureResult.DumpClientReturn(writer, indent + 1);
        writer.WriteLine(indent, "}");
    }

    public void DumpAbstractFunctionForServer(XdrFileWriter writer, int indent)
    {
        string arguments = _procedureArguments.GetArgumentsForClient();
        if (string.IsNullOrWhiteSpace(arguments))
        {
            arguments = "Caller caller";
        }
        else
        {
            arguments = "Caller caller, " + arguments;
        }

        writer.WriteLine(indent, $"public abstract {_procedureResult.Declaration} {FullName}({arguments});");
    }

    public void DumpStructsForServer(XdrFileWriter writer, int indent)
    {
        _procedureArguments.DumpStructForServer(writer, indent);
        _procedureResult.DumpStructForServer(writer, indent);
    }

    public void DumpCaseForServer(XdrFileWriter writer, int indent)
    {
        writer.WriteLine(indent, $"case {_argumentConstant}:");
        writer.WriteLine(indent, "{");
        _procedureArguments.DumpServerArgumentsCreation(writer, indent + 1);
        writer.WriteLine(indent + 1, $"call.RetrieveCall({_procedureArguments.VariableName});");
        writer.WriteLine(
            indent + 1,
            $"Settings?.Logger?.BeginCall({_versionConstant}, {_argumentConstant}, \"{Name}\", {_procedureArguments.VariableName});");
        _procedureResult.DumpServerResultCreation(writer, indent + 1);
        string result = _procedureResult.GetServerResult();
        string arguments = _procedureArguments.GetArgumentsForServer();
        if (!string.IsNullOrWhiteSpace(arguments))
        {
            arguments = ", " + arguments;
        }

        writer.WriteLine(indent + 1, "try");
        writer.WriteLine(indent + 1, "{");
        writer.WriteLine(indent + 2, $"{result}{FullName}(call.Caller{arguments});");
        writer.WriteLine(
            indent + 2,
            $"Settings?.Logger?.EndCall({_versionConstant}, {_argumentConstant}, \"{Name}\", {_procedureArguments.VariableName}, {_procedureResult.VariableName});");
        writer.WriteLine(indent + 2, "call.Reply(result);");
        writer.WriteLine(indent + 1, "}");
        writer.WriteLine(indent + 1, "catch (Exception exception) when (!(exception is RpcException))");
        writer.WriteLine(indent + 1, "{");
        writer.WriteLine(
            indent + 2,
            $"Settings?.Logger?.EndCall({_versionConstant}, {_argumentConstant}, \"{Name}\", {_procedureArguments.VariableName}, exception);");
        writer.WriteLine(indent + 2, "call.SystemError();");
        writer.WriteLine(indent + 2, "return;");
        writer.WriteLine(indent + 1, "}");

        writer.WriteLine(indent + 1, "break;");
        writer.WriteLine(indent, "}");
    }
}
