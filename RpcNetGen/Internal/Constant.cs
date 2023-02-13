// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

internal class Constant
{
    private readonly string _value;

    public Constant(RpcParser.ConstContext @const, Content content)
    {
        Name = @const.Identifier().GetText();
        _value = content.GetConstant(@const.constant());
    }

    public Constant(string name, string value)
    {
        Name = name;
        _value = value;
    }

    public string Name { get; }

    public void Dump(XdrFileWriter writer, int indent) =>
        writer.WriteLine(indent, $"public const int {Name} = {_value};");
}
