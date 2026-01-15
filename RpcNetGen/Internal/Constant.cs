// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

internal class Constant(string name, string value) : IEquatable<Constant>
{
    private readonly string _value = value;

    public Constant(RpcParser.ConstContext @const) : this(@const.Identifier().GetText(), Content.GetConstant(@const.constant()))
    {
    }

    public string Name { get; } = name;

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

        return (_value == other._value) && (Name == other.Name);
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
