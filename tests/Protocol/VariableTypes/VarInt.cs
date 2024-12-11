using System.Buffers;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Tests.Protocol.VariableTypes
{
    [TestClass]
    public sealed class VarIntTests
    {
        public static IEnumerable<object[]> VarIntData => new List<object[]>
        {
            new object[] { -2147483648, new byte[] { 0x80, 0x80, 0x80, 0x80, 0x08 } },
            new object[] { -1,          new byte[] { 0xff, 0xff, 0xff, 0xff, 0x0f } },
            new object[] { 2147483647,  new byte[] { 0xff, 0xff, 0xff, 0xff, 0x07 } },
            new object[] { 255,         new byte[] { 0xff, 0x01 } },
            new object[] { 128,         new byte[] { 0x80, 0x01 } },
            new object[] { 127,         new byte[] { 0x7f } },
            new object[] { 2,           new byte[] { 0x02 } },
            new object[] { 1,           new byte[] { 0x01 } },
            new object[] { 0,           new byte[] { 0x00 } },
        };

        [DataTestMethod]
        [DynamicData(nameof(VarIntData))]
        [Timeout(1000)]
        public void SerializeVarInt(int value, byte[] expected)
        {
            VarInt varInt = new()
            {
                Value = value,
                Length = expected.Length
            };

            byte[] target = new byte[VarInt.MaxLength];
            int length = varInt.Serialize(target);
            Assert.AreEqual(expected.Length, length);
            for (int i = 0; i < length; i++)
            {
                Assert.AreEqual(expected[i], target[i]);
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(VarIntData))]
        [Timeout(1000)]
        public void TryDeserializeVarInt(int expected, byte[] data)
        {
            SequenceReader<byte> reader = new(new ReadOnlySequence<byte>(data));
            Assert.IsTrue(VarInt.TryDeserialize(ref reader, out VarInt varInt));
            Assert.AreEqual(expected, varInt.Value);
            Assert.AreEqual(0, reader.Remaining);
            Assert.AreEqual(data.Length, reader.Consumed);
        }

        [DataTestMethod]
        [DynamicData(nameof(VarIntData))]
        [Timeout(1000)]
        public void DeserializeVarInt(int expected, byte[] data)
        {
            SequenceReader<byte> reader = new(new ReadOnlySequence<byte>(data));
            VarInt varInt = VarInt.Deserialize(ref reader);
            Assert.AreEqual(expected, varInt.Value);
            Assert.AreEqual(0, reader.Remaining);
            Assert.AreEqual(data.Length, reader.Consumed);
        }

        [DataTestMethod]
        [DynamicData(nameof(VarIntData))]
        [Timeout(1000)]
        public void ConstructorVarInt(int expected, byte[] data)
        {
            VarInt varInt = new(expected);
            Assert.AreEqual(expected, varInt.Value);
            Assert.AreEqual(data.Length, varInt.Length);
        }
    }
}
