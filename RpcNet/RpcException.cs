// Copyright by Artur Wolf

namespace RpcNet;

public sealed class RpcException(string message) : Exception(message);
