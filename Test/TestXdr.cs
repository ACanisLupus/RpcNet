namespace RpcNet.Test
{
    using NUnit.Framework;
    using RpcNet.Internal;

    internal class TestXdr
    {
        private readonly IXdrReader reader;
        private readonly StubNetwork stubNetwork;
        private readonly IXdrWriter writer;

        public TestXdr()
        {
            this.stubNetwork = new StubNetwork(65536, 65536);
            this.reader = new XdrReader(this.stubNetwork);
            this.writer = new XdrWriter(this.stubNetwork);
        }

        public static byte[] GenerateByteTestData(int length)
        {
            var value = new byte[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = (byte)i;
            }

            return value;
        }

        [SetUp]
        public void SetUp() => this.stubNetwork.Reset();

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
            writer.WriteVariableLengthOpaque(value);
            writer.Write(42);

            Assert.That(stubNetwork.WriteIndex, Is.EqualTo(32));
            Assert.That(reader.ReadOpaque(), Is.EqualTo(value));
            Assert.That(reader.ReadInt(), Is.EqualTo(42));
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

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(8));
            Assert.That(this.reader.ReadLong(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(ulong.MinValue)]
        [TestCase(42ul)]
        [TestCase(ulong.MaxValue)]
        public void ReadAndWriteULong(ulong value)
        {
            this.writer.Write(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(8));
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

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(4));
            Assert.That(this.reader.ReadInt(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(uint.MinValue)]
        [TestCase(42u)]
        [TestCase(uint.MaxValue)]
        public void ReadAndWriteUInt(uint value)
        {
            this.writer.Write(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(4));
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

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(4));
            Assert.That(this.reader.ReadShort(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(ushort.MinValue)]
        [TestCase((ushort)42)]
        [TestCase(ushort.MaxValue)]
        public void ReadAndWriteUShort(ushort value)
        {
            this.writer.Write(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(4));
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

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(4));
            Assert.That(this.reader.ReadSByte(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(byte.MinValue)]
        [TestCase((byte)42)]
        [TestCase(byte.MaxValue)]
        public void ReadAndWriteByte(byte value)
        {
            this.writer.Write(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(4));
            Assert.That(this.reader.ReadByte(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ReadAndWriteBool(bool value)
        {
            this.writer.Write(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(4));
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

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(4));
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

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(8));
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
            byte[] value = GenerateByteTestData(length);

            this.writer.WriteFixedLengthOpaque(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));

            var result = new byte[length];
            this.reader.ReadOpaque(result);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 4)]
        [TestCase(1, 8)]
        [TestCase(2, 8)]
        [TestCase(3, 8)]
        [TestCase(4, 8)]
        public void ReadAndWriteVariableLengthOpaque(int length, int expectedWriteIndex)
        {
            byte[] value = GenerateByteTestData(length);

            this.writer.WriteVariableLengthOpaque(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));
            Assert.That(this.reader.ReadOpaque(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(null, 4)]
        [TestCase("", 4)]
        [TestCase("a", 8)]
        [TestCase("ab", 8)]
        [TestCase("abc", 8)]
        [TestCase("abcd", 8)]
        public void ReadAndWriteString(string value, int expectedWriteIndex)
        {
            this.writer.Write(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));
            Assert.That(this.reader.ReadString(), Is.EqualTo(value ?? string.Empty));
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 4)]
        [TestCase(2, 8)]
        public void ReadAndWriteWriteFixedLengthBoolArray(int length, int expectedWriteIndex)
        {
            bool[] value = GenerateBoolTestData(length);

            this.writer.WriteFixedLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));

            var result = new bool[length];
            this.reader.ReadBoolArray(result);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 4)]
        [TestCase(1, 8)]
        [TestCase(2, 12)]
        public void ReadAndWriteWriteVariableLengthBoolArray(int length, int expectedWriteIndex)
        {
            bool[] value = GenerateBoolTestData(length);

            this.writer.WriteVariableLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));
            Assert.That(this.reader.ReadBoolArray(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 4)]
        [TestCase(2, 8)]
        public void ReadAndWriteWriteFixedLengthByteArray(int length, int expectedWriteIndex)
        {
            byte[] value = GenerateByteTestData(length);

            this.writer.WriteFixedLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));

            var result = new byte[length];
            this.reader.ReadByteArray(result);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 4)]
        [TestCase(1, 8)]
        [TestCase(2, 12)]
        public void ReadAndWriteWriteVariableLengthByteArray(int length, int expectedWriteIndex)
        {
            byte[] value = GenerateByteTestData(length);

            this.writer.WriteVariableLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));
            Assert.That(this.reader.ReadByteArray(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 4)]
        [TestCase(2, 8)]
        public void ReadAndWriteWriteFixedLengthSByteArray(int length, int expectedWriteIndex)
        {
            sbyte[] value = GenerateSByteTestData(length);

            this.writer.WriteFixedLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));

            var result = new sbyte[length];
            this.reader.ReadSByteArray(result);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 4)]
        [TestCase(1, 8)]
        [TestCase(2, 12)]
        public void ReadAndWriteWriteVariableLengthSByteArray(int length, int expectedWriteIndex)
        {
            sbyte[] value = GenerateSByteTestData(length);

            this.writer.WriteVariableLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));
            Assert.That(this.reader.ReadSByteArray(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 4)]
        [TestCase(2, 8)]
        public void ReadAndWriteWriteFixedLengthShortArray(int length, int expectedWriteIndex)
        {
            short[] value = GenerateShortTestData(length);

            this.writer.WriteFixedLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));

            var result = new short[length];
            this.reader.ReadShortArray(result);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 4)]
        [TestCase(1, 8)]
        [TestCase(2, 12)]
        public void ReadAndWriteWriteVariableLengthShortArray(int length, int expectedWriteIndex)
        {
            short[] value = GenerateShortTestData(length);

            this.writer.WriteVariableLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));
            Assert.That(this.reader.ReadShortArray(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 4)]
        [TestCase(2, 8)]
        public void ReadAndWriteWriteFixedLengthUShortArray(int length, int expectedWriteIndex)
        {
            ushort[] value = GenerateUShortTestData(length);

            this.writer.WriteFixedLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));

            var result = new ushort[length];
            this.reader.ReadUShortArray(result);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 4)]
        [TestCase(1, 8)]
        [TestCase(2, 12)]
        public void ReadAndWriteWriteVariableLengthUShortArray(int length, int expectedWriteIndex)
        {
            ushort[] value = GenerateUShortTestData(length);

            this.writer.WriteVariableLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));
            Assert.That(this.reader.ReadUShortArray(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 4)]
        [TestCase(2, 8)]
        public void ReadAndWriteWriteFixedLengthIntArray(int length, int expectedWriteIndex)
        {
            int[] value = GenerateIntTestData(length);

            this.writer.WriteFixedLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));

            var result = new int[length];
            this.reader.ReadIntArray(result);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 4)]
        [TestCase(1, 8)]
        [TestCase(2, 12)]
        public void ReadAndWriteWriteVariableLengthIntArray(int length, int expectedWriteIndex)
        {
            int[] value = GenerateIntTestData(length);

            this.writer.WriteVariableLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));
            Assert.That(this.reader.ReadIntArray(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 4)]
        [TestCase(2, 8)]
        public void ReadAndWriteWriteFixedLengthUIntArray(int length, int expectedWriteIndex)
        {
            uint[] value = GenerateUIntTestData(length);

            this.writer.WriteFixedLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));

            var result = new uint[length];
            this.reader.ReadUIntArray(result);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 4)]
        [TestCase(1, 8)]
        [TestCase(2, 12)]
        public void ReadAndWriteWriteVariableLengthUIntArray(int length, int expectedWriteIndex)
        {
            uint[] value = GenerateUIntTestData(length);

            this.writer.WriteVariableLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));
            Assert.That(this.reader.ReadUIntArray(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 8)]
        [TestCase(2, 16)]
        public void ReadAndWriteWriteFixedLengthLongArray(int length, int expectedWriteIndex)
        {
            long[] value = GenerateLongTestData(length);

            this.writer.WriteFixedLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));

            var result = new long[length];
            this.reader.ReadLongArray(result);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 4)]
        [TestCase(1, 12)]
        [TestCase(2, 20)]
        public void ReadAndWriteWriteVariableLengthLongArray(int length, int expectedWriteIndex)
        {
            long[] value = GenerateLongTestData(length);

            this.writer.WriteVariableLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));
            Assert.That(this.reader.ReadLongArray(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 8)]
        [TestCase(2, 16)]
        public void ReadAndWriteWriteFixedLengthULongArray(int length, int expectedWriteIndex)
        {
            ulong[] value = GenerateULongTestData(length);

            this.writer.WriteFixedLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));

            var result = new ulong[length];
            this.reader.ReadULongArray(result);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 4)]
        [TestCase(1, 12)]
        [TestCase(2, 20)]
        public void ReadAndWriteWriteVariableLengthULongArray(int length, int expectedWriteIndex)
        {
            ulong[] value = GenerateULongTestData(length);

            this.writer.WriteVariableLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));
            Assert.That(this.reader.ReadULongArray(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 4)]
        [TestCase(2, 8)]
        public void ReadAndWriteWriteFixedLengthFloatArray(int length, int expectedWriteIndex)
        {
            float[] value = GenerateFloatTestData(length);

            this.writer.WriteFixedLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));

            var result = new float[length];
            this.reader.ReadFloatArray(result);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 4)]
        [TestCase(1, 8)]
        [TestCase(2, 12)]
        public void ReadAndWriteWriteVariableLengthFloatArray(int length, int expectedWriteIndex)
        {
            float[] value = GenerateFloatTestData(length);

            this.writer.WriteVariableLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));
            Assert.That(this.reader.ReadFloatArray(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 8)]
        [TestCase(2, 16)]
        public void ReadAndWriteWriteFixedLengthDoubleArray(int length, int expectedWriteIndex)
        {
            double[] value = GenerateDoubleTestData(length);

            this.writer.WriteFixedLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));

            var result = new double[length];
            this.reader.ReadDoubleArray(result);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(0, 4)]
        [TestCase(1, 12)]
        [TestCase(2, 20)]
        public void ReadAndWriteWriteVariableLengthDoubleArray(int length, int expectedWriteIndex)
        {
            double[] value = GenerateDoubleTestData(length);

            this.writer.WriteVariableLengthArray(value);

            Assert.That(this.stubNetwork.WriteIndex, Is.EqualTo(expectedWriteIndex));
            Assert.That(this.reader.ReadDoubleArray(), Is.EqualTo(value));
        }

        private static bool[] GenerateBoolTestData(int length)
        {
            var value = new bool[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = i % 2 == 0;
            }

            return value;
        }

        private static sbyte[] GenerateSByteTestData(int length)
        {
            var value = new sbyte[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = (sbyte)i;
            }

            return value;
        }

        private static short[] GenerateShortTestData(int length)
        {
            var value = new short[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = (short)i;
            }

            return value;
        }

        private static ushort[] GenerateUShortTestData(int length)
        {
            var value = new ushort[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = (ushort)i;
            }

            return value;
        }

        private static int[] GenerateIntTestData(int length)
        {
            var value = new int[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = i;
            }

            return value;
        }

        private static uint[] GenerateUIntTestData(int length)
        {
            var value = new uint[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = (uint)i;
            }

            return value;
        }

        private static long[] GenerateLongTestData(int length)
        {
            var value = new long[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = i;
            }

            return value;
        }

        private static ulong[] GenerateULongTestData(int length)
        {
            var value = new ulong[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = (ulong)i;
            }

            return value;
        }

        private static float[] GenerateFloatTestData(int length)
        {
            var value = new float[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = i;
            }

            return value;
        }

        private static double[] GenerateDoubleTestData(int length)
        {
            var value = new double[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = i;
            }

            return value;
        }
    }
}
