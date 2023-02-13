// Copyright by Artur Wolf

namespace RpcNet;

using System.Text;

public class XdrVoid : IXdrDataType
{
    public void ReadFrom(IXdrReader reader)
    {
    }

    public void WriteTo(IXdrWriter writer)
    {
    }

    public void ToString(StringBuilder sb)
    {
    }

    public override string ToString() => "";
}
