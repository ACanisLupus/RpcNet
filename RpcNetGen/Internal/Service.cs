// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

internal class Service
{
    private readonly string _access;
    private readonly string _clientVersionNumber;
    private readonly string _name;
    private readonly List<Procedure> _parsedProcedures = new();
    private readonly string _programNumberConstant;
    private readonly List<string> _versionConstants = new();
    private readonly Dictionary<string, List<Procedure>> _parsedProceduresPerVersionConstant = new();

    public Service(string constantsClassName, RpcParser.ProgramContext program, string access, Content content)
    {
        _access = access;
        _name = content.Name;
        string programName = program.Identifier().GetText();
        string programNumber = content.GetConstant(program.constant());
        _programNumberConstant = content.AddConstant(programName, programNumber);
        foreach (RpcParser.VersionContext version in program.version())
        {
            string versionConstant = content.GetConstant(version.constant());
            string versionName = version.Identifier().GetText();
            string versionConstantFullName = content.AddConstant(versionName, versionConstant);
            _versionConstants.Add(versionConstantFullName);

            // Take the last one for client
            _clientVersionNumber = versionConstantFullName;

            foreach (RpcParser.ProcedureContext procedure in version.procedure())
            {
                var parsedProcedure = new Procedure(constantsClassName, versionConstant, versionConstantFullName, procedure, content);
                _parsedProcedures.Add(parsedProcedure);

                if (!_parsedProceduresPerVersionConstant.TryGetValue(versionConstantFullName, out List<Procedure> procedureList))
                {
                    procedureList = new List<Procedure>();
                    _parsedProceduresPerVersionConstant[versionConstantFullName] = procedureList;
                }

                procedureList.Add(parsedProcedure);
            }
        }
    }

    public void Prepare(Content content)
    {
        foreach (Procedure parsedProcedure in _parsedProcedures)
        {
            parsedProcedure.Prepare(content);
        }
    }

    public void Dump(XdrFileWriter writer, int indent)
    {
        DumpClient(writer, indent);
        DumpServer(writer, indent);
    }

    private void DumpClient(XdrFileWriter writer, int indent)
    {
        writer.WriteLine();
        writer.WriteLine(indent, $"{_access} class {_name}Client : ClientStub");
        writer.WriteLine(indent, "{");
        writer.WriteLine(indent + 1, $"public {_name}Client(Protocol protocol, IPAddress ipAddress, int port = 0, ClientSettings clientSettings = default) :");
        writer.WriteLine(indent + 2, $"base(protocol, ipAddress, port, {_programNumberConstant}, {_clientVersionNumber}, clientSettings)");
        writer.WriteLine(indent + 1, "{");
        writer.WriteLine(indent + 1, "}");
        foreach (Procedure procedure in _parsedProcedures)
        {
            procedure.DumpForClient(writer, indent + 1);
        }

        writer.WriteLine(indent, "}");
    }

    private void DumpServer(XdrFileWriter writer, int indent)
    {
        writer.WriteLine();
        writer.WriteLine(indent, $"{_access} abstract class {_name}ServerStub : ServerStub");
        writer.WriteLine(indent, "{");
        writer.WriteLine(
            indent + 1,
            $"public {_name}ServerStub(Protocol protocol, IPAddress ipAddress, int port = 0, ServerSettings serverSettings = default) :");
        string allVersions = string.Join(", ", _versionConstants);
        writer.WriteLine(indent + 2, $"base(protocol, ipAddress, port, {_programNumberConstant}, new[] {{ {allVersions} }}, serverSettings)");
        writer.WriteLine(indent + 1, "{");
        writer.WriteLine(indent + 1, "}");

        foreach (Procedure procedure in _parsedProcedures)
        {
            procedure.DumpStructsForServer(writer, indent + 1);
        }

        writer.WriteLine();

        foreach (Procedure procedure in _parsedProcedures)
        {
            procedure.DumpAbstractFunctionForServer(writer, indent + 1);
        }

        writer.WriteLine();

        writer.WriteLine(indent + 1, "protected override void DispatchReceivedCall(ReceivedRpcCall call)");
        writer.WriteLine(indent + 1, "{");
        bool first = true;
        foreach (string versionConstant in _versionConstants)
        {
            if (first)
            {
                writer.WriteLine(indent + 2, $"if (call.Version == {versionConstant})");
                first = false;
            }
            else
            {
                writer.WriteLine(indent + 2, $"else if (call.Version == {versionConstant})");
            }

            writer.WriteLine(indent + 2, "{");
            writer.WriteLine(indent + 3, "switch (call.Procedure)");
            writer.WriteLine(indent + 3, "{");
            foreach (Procedure procedure in _parsedProceduresPerVersionConstant[versionConstant])
            {
                procedure.DumpCaseForServer(writer, indent + 4);
            }

            writer.WriteLine(indent + 4, "default:");
            writer.WriteLine(indent + 5, "Settings?.Logger?.Error($\"Procedure unavailable (Version: {call.Version}, Procedure: {call.Procedure}).\");");
            writer.WriteLine(indent + 5, "call.ProcedureUnavailable();");
            writer.WriteLine(indent + 5, "break;");
            writer.WriteLine(indent + 3, "}");
            writer.WriteLine(indent + 2, "}");
        }

        writer.WriteLine(indent + 2, "else");

        writer.WriteLine(indent + 2, "{");
        writer.WriteLine(indent + 3, "Settings?.Logger?.Error($\"Program mismatch (Version: {call.Version}).\");");
        writer.WriteLine(indent + 3, "call.ProgramMismatch();");
        writer.WriteLine(indent + 2, "}");

        writer.WriteLine(indent + 1, "}");

        writer.WriteLine(indent, "}");
    }
}
