// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

internal class EnumerationValue(RpcParser.EnumValueContext enumValue)
{
    private string Name { get; } = enumValue.Identifier().GetText();
    private string Value { get; } = Content.GetValue(enumValue.value());

    public void Dump(XdrFileWriter writer, int indent)
    {
        if (Value is not null)
        {
            writer.WriteLine(indent, $"{Name} = {Value},");
        }
        else
        {
            writer.WriteLine(indent, $"{Name},");
        }
    }
}
