// Copyright by Artur Wolf

namespace RpcNet;

public interface ILogger
{
    void Trace(string entry);
    void Info(string entry);
    void Error(string entry);
    void BeginCall(int version, int procedure, string method, IXdrDataType arguments);
    void EndCall(int version, int procedure, string method, IXdrDataType arguments, IXdrDataType result);
    void EndCall(int version, int procedure, string method, IXdrDataType arguments, Exception exception);
}
