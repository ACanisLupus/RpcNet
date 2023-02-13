// Copyright by Artur Wolf

namespace RpcNet;

using System.Text;

public interface IXdrDataType
{
    void ReadFrom(IXdrReader reader);
    void WriteTo(IXdrWriter writer);
    void ToString(StringBuilder sb);
}
