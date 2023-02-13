// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

internal enum DataTypeKind
{
    Unknown,
    Simple,
    Enum,
    CustomType,
    Opaque,
    Void
}
