// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

internal class Constant : IEquatable<Constant>
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

    public void Dump(XdrFileWriter writer, int indent) => writer.WriteLine(indent, $"public const int {Name} = {_value};");

    public override string ToString() => $"{{ Name = {Name}, Value = {_value} }}";

    public bool Equals(Constant other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _value == other._value && Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((Constant)obj);
    }

    public override int GetHashCode() => HashCode.Combine(_value, Name);
}
