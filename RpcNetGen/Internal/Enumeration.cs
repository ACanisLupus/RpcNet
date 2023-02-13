// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

internal class Enumeration
{
    private readonly string _access;
    private readonly List<EnumerationValue> _enumItems = new();

    public Enumeration(RpcParser.EnumContext @enum, string access, Content content)
    {
        _access = access;
        Name = @enum.Identifier().GetText();

        RpcParser.EnumValueContext[] enumValues = @enum.enumValue();
        foreach (RpcParser.EnumValueContext enumValue in enumValues)
        {
            _enumItems.Add(new EnumerationValue(enumValue, content));
        }
    }

    public string Name { get; }
    public IReadOnlyList<EnumerationValue> EnumItems => _enumItems;

    public void Dump(XdrFileWriter writer, int indent)
    {
        writer.WriteLine();
        writer.WriteLine(indent, $"{_access} enum {Name}");
        writer.WriteLine(indent, "{");
        foreach (EnumerationValue enumItem in EnumItems)
        {
            enumItem.Dump(writer, indent + 1);
        }

        writer.WriteLine(indent, "}");
    }
}
