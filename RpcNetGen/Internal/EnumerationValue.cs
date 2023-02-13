// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

internal class EnumerationValue
{
    public EnumerationValue(RpcParser.EnumValueContext enumValue, Content content)
    {
        Name = enumValue.Identifier().GetText();
        Value = content.GetValue(enumValue.value());
    }

    public string Name { get; }
    public string Value { get; }

    public void Dump(XdrFileWriter writer, int indent)
    {
        if (Value != null)
        {
            writer.WriteLine(indent, $"{Name} = {Value},");
        }
        else
        {
            writer.WriteLine(indent, $"{Name},");
        }
    }
}
