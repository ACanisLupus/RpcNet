namespace Rpc.Net.Test
{
    using System.IO;
    using NUnit.Framework;
    using RpcNet;
    using RpcNet.Internal;

    class TestXdr
    {
        private readonly MemoryStream memoryStream = new MemoryStream(new byte[65536]);
        private readonly IBufferReader bufferReader;
        private readonly IBufferWriter bufferWriter;
        private readonly IXdrReader reader;
        private readonly IXdrWriter writer;

        public TestXdr()
        {
            this.bufferReader = new UdpBufferReader(this.memoryStream);
            this.bufferWriter = new UdpBufferWriter(this.memoryStream);
            this.reader = new XdrReader(this.bufferReader);
            this.writer = new XdrWriter(this.bufferWriter);
        }

        [SetUp]
        public void SetUp()
        {
            this.memoryStream.Position = 0;
            this.bufferWriter.BeginWriting();
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

            this.Copy();

            Assert.That(this.reader.ReadLong(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(ulong.MinValue)]
        [TestCase(42ul)]
        [TestCase(ulong.MaxValue)]
        public void ReadAndWriteULong(ulong value)
        {
            this.writer.Write(value);

            this.Copy();

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

            this.Copy();

            Assert.That(this.reader.ReadInt(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(uint.MinValue)]
        [TestCase(42u)]
        [TestCase(uint.MaxValue)]
        public void ReadAndWriteUInt(uint value)
        {
            this.writer.Write(value);

            this.Copy();

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

            this.Copy();

            Assert.That(this.reader.ReadShort(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(ushort.MinValue)]
        [TestCase((ushort)42)]
        [TestCase(ushort.MaxValue)]
        public void ReadAndWriteUShort(ushort value)
        {
            this.writer.Write(value);

            this.Copy();

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

            this.Copy();

            Assert.That(this.reader.ReadSByte(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(byte.MinValue)]
        [TestCase((byte)42)]
        [TestCase(byte.MaxValue)]
        public void ReadAndWriteByte(byte value)
        {
            this.writer.Write(value);

            this.Copy();

            Assert.That(this.reader.ReadByte(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ReadAndWriteBool(bool value)
        {
            this.writer.Write(value);

            this.Copy();

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

            this.Copy();

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

            this.Copy();

            Assert.That(this.reader.ReadDouble(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void ReadAndWriteFixedLengthOpaque(int length)
        {
            byte[] value = new byte[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = (byte)(i + 1);
            }

            this.writer.WriteFixedLengthOpaque(value);

            this.Copy();

            Assert.That(this.reader.ReadOpaque(length), Is.EqualTo(value));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void ReadAndWriteVariableLengthOpaque(int length)
        {
            byte[] value = new byte[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = (byte)(i + 1);
            }

            this.writer.WriteVariableLengthOpaque(value);

            this.Copy();

            Assert.That(this.reader.ReadOpaque(), Is.EqualTo(value));
        }

        [Test]
        [TestCase("")]
        [TestCase("a")]
        [TestCase("ab")]
        [TestCase("abc")]
        [TestCase("abcd")]
        public void ReadAndWriteString(string value)
        {
            this.writer.Write(value);

            this.Copy();

            Assert.That(this.reader.ReadString(), Is.EqualTo(value));
        }

        private void Copy()
        {
            this.bufferWriter.EndWriting();
            Assert.That(this.memoryStream.Position % 4, Is.EqualTo(0), "Position must be a multiply of 4.");
            this.memoryStream.Position = 0;
            this.bufferReader.BeginReading();
        }
    }
}
