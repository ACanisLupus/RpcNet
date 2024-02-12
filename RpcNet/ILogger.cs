// Copyright by Artur Wolf

namespace RpcNet;

public interface ILogger
{
    void Trace(string entry);
    void Info(string entry);
    void Error(string entry);
    void BeginCall(RpcEndPoint rpcEndPoint, int version, int procedure, string method, IXdrDataType arguments);
    void EndCall(RpcEndPoint rpcEndPoint, int version, int procedure, string method, IXdrDataType arguments, IXdrDataType result);
    void EndCall(RpcEndPoint rpcEndPoint, int version, int procedure, string method, IXdrDataType arguments, Exception exception);
}
