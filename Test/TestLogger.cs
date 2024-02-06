// Copyright by Artur Wolf

namespace Test;

using RpcNet;

internal sealed class TestLogger : ILogger
{
    private readonly string _name;

    public TestLogger(string name) => _name = name;

    public void Trace(string entry) => Console.WriteLine($"[{_name}] [TRACE] {entry}");
    public void Info(string entry) => Console.WriteLine($"[{_name}] [INFO]  {entry}");
    public void Error(string entry) => Console.WriteLine($"[{_name}] [ERROR] {entry}");
    public void BeginCall(int version, int procedure, string method, IXdrDataType arguments) => Console.WriteLine($"[{_name}] [BEGIN] {method}({arguments})");

    public void EndCall(int version, int procedure, string method, IXdrDataType arguments, IXdrDataType result) =>
        Console.WriteLine($"[{_name}] [END]   {method}({arguments}): {result}");

    public void EndCall(int version, int procedure, string method, IXdrDataType arguments, Exception exception) =>
        Console.WriteLine($"[{_name}] [END]   {method}({arguments}): {exception}");
}
