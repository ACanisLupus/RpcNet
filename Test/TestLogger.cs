// Copyright by Artur Wolf

namespace Test;

using RpcNet;

internal sealed class TestLogger(string name) : ILogger
{
    public void Trace(string entry) => Console.WriteLine($"[{name}] [TRACE] {entry}");
    public void Info(string entry) => Console.WriteLine($"[{name}] [INFO]  {entry}");
    public void Error(string entry) => Console.WriteLine($"[{name}] [ERROR] {entry}");

    public void BeginCall(RpcEndPoint rpcEndPoint, int version, int procedure, string method, IXdrDataType arguments) =>
        Console.WriteLine($"[{name}] [BEGIN] {method}({arguments})");

    public void EndCall(RpcEndPoint rpcEndPoint, int version, int procedure, string method, IXdrDataType arguments, IXdrDataType result) =>
        Console.WriteLine($"[{name}] [END]   {method}({arguments}): {result}");

    public void EndCall(RpcEndPoint rpcEndPoint, int version, int procedure, string method, IXdrDataType arguments, Exception exception) =>
        Console.WriteLine($"[{name}] [END]   {method}({arguments}): {exception}");
}
