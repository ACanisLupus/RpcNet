// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

internal class Procedure
{
    private readonly string _argumentConstant;
    private readonly ProcedureArguments _procedureArguments;
    private readonly ProcedureResult _procedureResult;
    private readonly string _versionConstant;

    public Procedure(Settings settings, string version, string versionConstant, RpcParser.ProcedureContext procedureContext, Content content)
    {
        _versionConstant = versionConstant;
        Name = procedureContext.Identifier().GetText();
        FullName = Name + "_" + version;
        _argumentConstant = content.AddConstant(Name, Content.GetConstant(procedureContext.constant()));

        _procedureArguments = new ProcedureArguments(settings, procedureContext.arguments(), FullName);
        _procedureResult = new ProcedureResult(settings, procedureContext.@return(), FullName);
    }

    private string Name { get; }
    private string FullName { get; }

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
        if (_procedureResult.IsVoid)
        {
            writer.WriteLine(indent, $"public async ValueTask {FullName}Async({_procedureArguments.GetArgumentsForClient()})");
        }
        else
        {
            writer.WriteLine(indent, $"public async ValueTask<{_procedureResult.Declaration}> {FullName}Async({_procedureArguments.GetArgumentsForClient()})");
        }

        writer.WriteLine(indent, "{");
        _procedureArguments.DumpClientArgumentsCreation(writer, indent + 1);
        _procedureResult.DumpClientResultCreation(writer, indent + 1);
        writer.WriteLine(
            indent + 1,
            $"Settings.Logger?.BeginCall(RpcEndPoint, {_versionConstant}, {_argumentConstant}, \"{FullName}\", {_procedureArguments.VariableName});");
        writer.WriteLine(indent + 1, "try");
        writer.WriteLine(indent + 1, "{");
        writer.WriteLine(
            indent + 2,
            $"await CallAsync({_argumentConstant}, {_versionConstant}, {_procedureArguments.VariableName}, {ProcedureResult.VariableName}, cancellationToken).ConfigureAwait(false);");
        writer.WriteLine(indent + 1, "}");
        writer.WriteLine(indent + 1, "catch (Exception e)");
        writer.WriteLine(indent + 1, "{");
        writer.WriteLine(
            indent + 2,
            $"Settings.Logger?.EndCall(RpcEndPoint, {_versionConstant}, {_argumentConstant}, \"{FullName}\", {_procedureArguments.VariableName}, e);");
        writer.WriteLine(indent + 2, "throw;");
        writer.WriteLine(indent + 1, "}");
        writer.WriteLine(
            indent + 1,
            $"Settings.Logger?.EndCall(RpcEndPoint, {_versionConstant}, {_argumentConstant}, \"{FullName}\", {_procedureArguments.VariableName}, {ProcedureResult.VariableName});");
        _procedureResult.DumpClientReturn(writer, indent + 1);
        writer.WriteLine(indent, "}");
    }

    public void DumpAbstractFunctionForServer(XdrFileWriter writer, int indent)
    {
        string arguments = _procedureArguments.GetArgumentsForClientInServer();
        arguments = string.IsNullOrWhiteSpace(arguments) ? "RpcEndPoint rpcEndPoint" : "RpcEndPoint rpcEndPoint, " + arguments;

        if (_procedureResult.IsVoid)
        {
            writer.WriteLine(indent, $"public abstract ValueTask {FullName}Async({arguments});");
        }
        else
        {
            writer.WriteLine(indent, $"public abstract ValueTask<{_procedureResult.Declaration}> {FullName}Async({arguments});");
        }
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
            $"Settings.Logger?.BeginCall(call.RpcEndPoint, {_versionConstant}, {_argumentConstant}, \"{FullName}\", {_procedureArguments.VariableName});");
        _procedureResult.DumpServerResultCreation(writer, indent + 1);
        string result = _procedureResult.GetServerResult();
        string arguments = _procedureArguments.GetArgumentsForServer();
        if (!string.IsNullOrWhiteSpace(arguments))
        {
            arguments = ", " + arguments;
        }

        writer.WriteLine(indent + 1, "try");
        writer.WriteLine(indent + 1, "{");
        writer.WriteLine(indent + 2, $"{result}await {FullName}Async(call.RpcEndPoint{arguments}, cancellationToken).ConfigureAwait(false);");
        writer.WriteLine(
            indent + 2,
            $"Settings.Logger?.EndCall(call.RpcEndPoint, {_versionConstant}, {_argumentConstant}, \"{FullName}\", {_procedureArguments.VariableName}, {ProcedureResult.VariableName});");
        writer.WriteLine(indent + 2, "call.Reply(result);");
        writer.WriteLine(indent + 1, "}");
        writer.WriteLine(indent + 1, "catch (Exception e) when (!(e is RpcException))");
        writer.WriteLine(indent + 1, "{");
        writer.WriteLine(
            indent + 2,
            $"Settings.Logger?.EndCall(call.RpcEndPoint, {_versionConstant}, {_argumentConstant}, \"{FullName}\", {_procedureArguments.VariableName}, e);");
        writer.WriteLine(indent + 2, "call.SystemError();");
        writer.WriteLine(indent + 2, "return;");
        writer.WriteLine(indent + 1, "}");

        writer.WriteLine(indent + 1, "break;");
        writer.WriteLine(indent, "}");
    }
}
