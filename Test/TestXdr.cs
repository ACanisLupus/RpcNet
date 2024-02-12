// Copyright by Artur Wolf

namespace Test;

using NUnit.Framework;
using RpcNet;
using RpcNet.Internal;

[TestFixture]
internal sealed class TestXdr
{
    private readonly IXdrReader _reader;
    private readonly StubNetwork _stubNetwork;
    private readonly IXdrWriter _writer;

    public TestXdr()
    {
        _stubNetwork = new StubNetwork(65536, 65536);
        _reader = new XdrReader(_stubNetwork);
        _writer = new XdrWriter(_stubNetwork);
    }

    public static byte[] GenerateByteTestData(int length)
    {
        byte[] value = new byte[length];
        for (int i = 0; i < length; i++)
        {
            value[i] = (byte)i;
        }

        return value;
    }

    [SetUp]
    public void SetUp() => _stubNetwork.Reset();

    [Test]
    [TestCase(28, 28)]
    [TestCase(28, 4)]
    [TestCase(4, 28)]
    [TestCase(4, 4)]
    [TestCase(8, 4)]
    [TestCase(4, 8)]
    public void ReserveMultipleFragmentsOpaque(int maxReadLength, int maxReserveLength)
    {
        var stubNetwork = new StubNetwork(maxReadLength, maxReserveLength);
        var reader = new XdrReader(stubNetwork);
        var writer = new XdrWriter(stubNetwork);

        byte[] value = GenerateByteTestData(21);
        writer.WriteOpaque(value);
        writer.Write(42);

        Assert.That(stubNetwork.WriteIndex, Is.EqualTo(32));
        Assert.That(reader.ReadOpaque(), Is.EqualTo(value));
        Assert.That(reader.ReadInt32(), Is.EqualTo(42));
    }

    [Test]
    [TestCase(0)]
    [TestCase(42)]
    [TestCase(-12)]
    [TestCase(long.MaxValue)]
    [TestCase(long.MinValue)]
    public void ReadAndWriteLong(long value)
    {
        _writer.Write(value);

        Assert.That(_stubNetwork.WriteIndex, Is.EqualTo(8));
        Assert.That(_reader.ReadInt64(), Is.EqualTo(value));
    }

    [Test]
    [TestCase(ulong.MinValue)]
    [TestCase(42ul)]
    [TestCase(ulong.MaxValue)]
    public void ReadAndWriteULong(ulong value)
    {
        _writer.Write(value);

        Assert.That(_stubNetwork.WriteIndex, Is.EqualTo(8));
        Assert.That(_reader.ReadUInt64(), Is.EqualTo(value));
    }

    [Test]
    [TestCase(0)]
    [TestCase(42)]
    [TestCase(-12)]
    [TestCase(int.MaxValue)]
    [TestCase(int.MinValue)]
    public void ReadAndWriteInt(int value)
    {
        _writer.Write(value);

        Assert.That(_stubNetwork.WriteIndex, Is.EqualTo(4));
        Assert.That(_reader.ReadInt32(), Is.EqualTo(value));
    }

    [Test]
    [TestCase(uint.MinValue)]
    [TestCase(42u)]
    [TestCase(uint.MaxValue)]
    public void ReadAndWriteUInt(uint value)
    {
        _writer.Write(value);

        Assert.That(_stubNetwork.WriteIndex, Is.EqualTo(4));
        Assert.That(_reader.ReadUInt32(), Is.EqualTo(value));
    }

    [Test]
    [TestCase((short)0)]
    [TestCase((short)42)]
    [TestCase((short)-12)]
    [TestCase(short.MaxValue)]
    [TestCase(short.MinValue)]
    public void ReadAndWriteShort(short value)
    {
        _writer.Write(value);

        Assert.That(_stubNetwork.WriteIndex, Is.EqualTo(4));
        Assert.That(_reader.ReadInt16(), Is.EqualTo(value));
    }

    [Test]
    [TestCase(ushort.MinValue)]
    [TestCase((ushort)42)]
    [TestCase(ushort.MaxValue)]
    public void ReadAndWriteUShort(ushort value)
    {
        _writer.Write(value);

        Assert.That(_stubNetwork.WriteIndex, Is.EqualTo(4));
        Assert.That(_reader.ReadUInt16(), Is.EqualTo(value));
    }

    [Test]
    [TestCase((sbyte)0)]
    [TestCase((sbyte)42)]
    [TestCase((sbyte)-12)]
    [TestCase(sbyte.MaxValue)]
    [TestCase(sbyte.MinValue)]
    public void ReadAndWriteSByte(sbyte value)
    {
        _writer.Write(value);

        Assert.That(_stubNetwork.WriteIndex, Is.EqualTo(4));
        Assert.That(_reader.ReadInt8(), Is.EqualTo(value));
    }

    [Test]
    [TestCase(byte.MinValue)]
    [TestCase((byte)42)]
    [TestCase(byte.MaxValue)]
    public void ReadAndWriteByte(byte value)
    {
        _writer.Write(value);

        Assert.That(_stubNetwork.WriteIndex, Is.EqualTo(4));
        Assert.That(_reader.ReadUInt8(), Is.EqualTo(value));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ReadAndWriteBool(bool value)
    {
        _writer.Write(value);

        Assert.That(_stubNetwork.WriteIndex, Is.EqualTo(4));
        Assert.That(_reader.ReadBool(), Is.EqualTo(value));
    }

    [Test]
    [TestCase(0.0f)]
    [TestCase(4.2f)]
    [TestCase(-1.2f)]
    [TestCase(float.MaxValue)]
    [TestCase(float.MinValue)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    [TestCase(float.Epsilon)]
    [TestCase(float.NaN)]
    public void ReadAndWriteFloat(float value)
    {
        _writer.Write(value);

        Assert.That(_stubNetwork.WriteIndex, Is.EqualTo(4));
        Assert.That(_reader.ReadFloat32(), Is.EqualTo(value));
    }

    [Test]
    [TestCase(0.0)]
    [TestCase(4.2)]
    [TestCase(-1.2)]
    [TestCase(double.MaxValue)]
    [TestCase(double.MinValue)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    [TestCase(double.Epsilon)]
    [TestCase(double.NaN)]
    public void ReadAndWriteDouble(double value)
    {
        _writer.Write(value);

        Assert.That(_stubNetwork.WriteIndex, Is.EqualTo(8));
        Assert.That(_reader.ReadFloat64(), Is.EqualTo(value));
    }

    [Test]
    [TestCase(0, 4)]
    [TestCase(1, 8)]
    [TestCase(2, 8)]
    [TestCase(3, 8)]
    [TestCase(4, 8)]
    public void ReadAndWriteOpaque(int length, int expectedWriteIndex)
    {
        byte[] value = GenerateByteTestData(length);

        _writer.WriteOpaque(value);

        Assert.That(_stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));
        Assert.That(_reader.ReadOpaque(), Is.EqualTo(value));
    }

    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 4)]
    [TestCase(2, 4)]
    [TestCase(3, 4)]
    [TestCase(4, 4)]
    public void ReadAndWriteFixedLengthOpaque(int length, int expectedWriteIndex)
    {
        byte[] writeValue = GenerateByteTestData(length);
        byte[] readValue = new byte[length];

        _writer.WriteFixedLengthOpaque(writeValue);
        _reader.ReadFixedLengthOpaque(readValue);

        Assert.That(_stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));
        Assert.That(readValue, Is.EqualTo(writeValue));
    }

    [Test]
    [TestCase(null, 4)]
    [TestCase("", 4)]
    [TestCase("a", 8)]
    [TestCase("ab", 8)]
    [TestCase("abc", 8)]
    [TestCase("text", 8)]
    public void ReadAndWriteString(string value, int expectedWriteIndex)
    {
        _writer.Write(value);

        Assert.That(_stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));
        Assert.That(_reader.ReadString(), Is.EqualTo(value ?? string.Empty));
    }
}
