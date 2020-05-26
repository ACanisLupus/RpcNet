namespace RpcNet.Test
{
    using System.IO;
    using NUnit.Framework;
    using RpcNet;
    using RpcNet.Internal;

    class TestXdr
    {
        private readonly StubNetwork stubNetwork;
        private readonly IXdrReader reader;
        private readonly IXdrWriter writer;

        public TestXdr()
        {
            this.stubNetwork = new StubNetwork();
            this.reader = new XdrReader(this.stubNetwork);
            this.writer = new XdrWriter(this.stubNetwork);
        }

        [SetUp]
        public void SetUp() => this.stubNetwork.Reset();

        [Test]
        public void ReadAndWriteInt()
        {
            this.writer.Write(42);

            this.AssertWriteIndex(4);

            Assert.That(this.reader.ReadInt(), Is.EqualTo(42));
        }

        [Test]
        [TestCase(0)]
        [TestCase(42)]
        [TestCase(-12)]
        [TestCase(long.MaxValue)]
        [TestCase(long.MinValue)]
        public void ReadAndWriteLong(long value)
        {
            this.writer.Write(value);

            this.AssertWriteIndex(8);

            Assert.That(this.reader.ReadLong(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(ulong.MinValue)]
        [TestCase(42ul)]
        [TestCase(ulong.MaxValue)]
        public void ReadAndWriteULong(ulong value)
        {
            this.writer.Write(value);

            this.AssertWriteIndex(8);

            Assert.That(this.reader.ReadULong(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(0)]
        [TestCase(42)]
        [TestCase(-12)]
        [TestCase(int.MaxValue)]
        [TestCase(int.MinValue)]
        public void ReadAndWriteInt(int value)
        {
            this.writer.Write(value);

            this.AssertWriteIndex(4);

            Assert.That(this.reader.ReadInt(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(uint.MinValue)]
        [TestCase(42u)]
        [TestCase(uint.MaxValue)]
        public void ReadAndWriteUInt(uint value)
        {
            this.writer.Write(value);

            this.AssertWriteIndex(4);

            Assert.That(this.reader.ReadUInt(), Is.EqualTo(value));
        }

        [Test]
        [TestCase((short)0)]
        [TestCase((short)42)]
        [TestCase((short)-12)]
        [TestCase(short.MaxValue)]
        [TestCase(short.MinValue)]
        public void ReadAndWriteShort(short value)
        {
            this.writer.Write(value);

            this.AssertWriteIndex(4);

            Assert.That(this.reader.ReadShort(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(ushort.MinValue)]
        [TestCase((ushort)42)]
        [TestCase(ushort.MaxValue)]
        public void ReadAndWriteUShort(ushort value)
        {
            this.writer.Write(value);

            this.AssertWriteIndex(4);

            Assert.That(this.reader.ReadUShort(), Is.EqualTo(value));
        }

        [Test]
        [TestCase((sbyte)0)]
        [TestCase((sbyte)42)]
        [TestCase((sbyte)-12)]
        [TestCase(sbyte.MaxValue)]
        [TestCase(sbyte.MinValue)]
        public void ReadAndWriteSByte(sbyte value)
        {
            this.writer.Write(value);

            this.AssertWriteIndex(4);

            Assert.That(this.reader.ReadSByte(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(byte.MinValue)]
        [TestCase((byte)42)]
        [TestCase(byte.MaxValue)]
        public void ReadAndWriteByte(byte value)
        {
            this.writer.Write(value);

            this.AssertWriteIndex(4);

            Assert.That(this.reader.ReadByte(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ReadAndWriteBool(bool value)
        {
            this.writer.Write(value);

            this.AssertWriteIndex(4);

            Assert.That(this.reader.ReadBool(), Is.EqualTo(value));
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
            this.writer.Write(value);

            this.AssertWriteIndex(4);

            Assert.That(this.reader.ReadFloat(), Is.EqualTo(value));
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
            this.writer.Write(value);

            this.AssertWriteIndex(8);

            Assert.That(this.reader.ReadDouble(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 4)]
        [TestCase(2, 4)]
        [TestCase(3, 4)]
        [TestCase(4, 4)]
        public void ReadAndWriteFixedLengthOpaque(int length, int expectedWriteIndex)
        {
            byte[] value = new byte[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = (byte)(i + 1);
            }

            this.writer.WriteFixedLengthOpaque(value);

            this.AssertWriteIndex(expectedWriteIndex);

            Assert.That(this.reader.ReadOpaque(length), Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 4)]
        [TestCase(1, 8)]
        [TestCase(2, 8)]
        [TestCase(3, 8)]
        [TestCase(4, 8)]
        public void ReadAndWriteVariableLengthOpaque(int length, int expectedWriteIndex)
        {
            byte[] value = new byte[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = (byte)(i + 1);
            }

            this.writer.WriteVariableLengthOpaque(value);

            this.AssertWriteIndex(expectedWriteIndex);

            Assert.That(this.reader.ReadOpaque(), Is.EqualTo(value));
        }

        [Test]
        [TestCase("", 4)]
        [TestCase("a", 8)]
        [TestCase("ab", 8)]
        [TestCase("abc", 8)]
        [TestCase("abcd", 8)]
        public void ReadAndWriteString(string value, int expectedWriteIndex)
        {
            this.writer.Write(value);

            this.AssertWriteIndex(expectedWriteIndex);

            Assert.That(this.reader.ReadString(), Is.EqualTo(value));
        }

        private void AssertWriteIndex(int expectedWriteIndex) => Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));
    }
}
