using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Protocol.Net
{
    public static partial class SequenceReaderExtensions
    {
        public static bool TryReadByte(this ref SequenceReader<byte> reader, out byte value) => reader.TryRead(out value);

        public static bool TryReadSignedByte(this ref SequenceReader<byte> reader, out sbyte value)
        {
            if (reader.TryRead(out byte byteValue))
            {
                value = unchecked((sbyte)byteValue);
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryReadShort(this ref SequenceReader<byte> reader, out short value) => reader.TryReadBigEndian(out value);

        public static bool TryReadUnsignedShort(this ref SequenceReader<byte> reader, out ushort value)
        {
            if (reader.TryReadBigEndian(out short shortValue))
            {
                value = unchecked((ushort)shortValue);
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryReadInt(this ref SequenceReader<byte> reader, out int value) => reader.TryReadBigEndian(out value);

        public static bool TryReadUnsignedInt(this ref SequenceReader<byte> reader, out uint value)
        {
            if (reader.TryReadBigEndian(out int intValue))
            {
                value = unchecked((uint)intValue);
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryReadLong(this ref SequenceReader<byte> reader, out long value) => reader.TryReadBigEndian(out value);

        public static bool TryReadUnsignedLong(this ref SequenceReader<byte> reader, out ulong value)
        {
            if (reader.TryReadBigEndian(out long longValue))
            {
                value = unchecked((ulong)longValue);
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryReadFloat(this ref SequenceReader<byte> reader, out float value)
        {
            value = default;
            return reader.TryReadExact(4, out ReadOnlySequence<byte> sequence)
                && float.TryParse(sequence.IsSingleSegment ? sequence.First.Span : sequence.ToArray(), CultureInfo.InvariantCulture, out value);
        }

        public static bool TryReadDouble(this ref SequenceReader<byte> reader, out double value)
        {
            value = default;
            return reader.TryReadExact(8, out ReadOnlySequence<byte> sequence)
                && double.TryParse(sequence.IsSingleSegment ? sequence.First.Span : sequence.ToArray(), CultureInfo.InvariantCulture, out value);
        }

        public static bool TryReadString(this ref SequenceReader<byte> reader, [NotNullWhen(true)] out string? value)
        {
            if (TryReadVarInt(ref reader, out VarInt length) && reader.TryReadExact(length.Value, out ReadOnlySequence<byte> sequence))
            {
                value = Encoding.UTF8.GetString(sequence.IsSingleSegment ? sequence.First.Span : sequence.ToArray());
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryReadVarInt(this ref SequenceReader<byte> reader, out VarInt value) => VarInt.TryDeserialize(ref reader, out value);
        public static bool TryReadVarLong(this ref SequenceReader<byte> reader, out VarLong value) => VarLong.TryDeserialize(ref reader, out value);
    }
}
