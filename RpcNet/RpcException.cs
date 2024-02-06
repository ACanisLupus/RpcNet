// Copyright by Artur Wolf

namespace RpcNet;

public sealed class RpcException : Exception
{
    public RpcException(string message) : base(message)
    {
    }
}
