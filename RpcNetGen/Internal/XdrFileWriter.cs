// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

internal class XdrFileWriter : IDisposable
{
    private readonly TextWriter _writer;

    public XdrFileWriter(string filePath) => _writer = new StreamWriter(filePath);

    public void WriteLine(int indent, string line) => _writer.Write(GetIndentString(indent) + line + '\n');
    public void WriteLine() => _writer.Write('\n');
    public void Dispose() => _writer?.Dispose();
    private static string GetIndentString(int indent) => new(' ', indent * 4);
}
