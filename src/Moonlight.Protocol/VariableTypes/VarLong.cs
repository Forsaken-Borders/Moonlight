using System;
using System.Buffers;

namespace Moonlight.Protocol.VariableTypes
{
    public readonly record struct VarLong : ISpanSerializable<VarLong>
    {
        public long Value { get; init; }
        public int Length { get; init; }

        private const int SEGMENT_BITS = 0x7F;
        private const int CONTINUE_BIT = 0x80;

        public VarLong(long value)
        {
            Value = value;
            do
            {
                value >>= 7;
                Length++;
            } while (value != 0);
        }

        public int Serialize(Span<byte> target)
        {
            target.Clear();
            long value = Value;
            int position = 0;
            do
            {
                byte bit = (byte)(value & SEGMENT_BITS);
                value >>= 7;
                if (value != 0)
                {
                    bit |= CONTINUE_BIT;
                }

                target[position] = bit;
                position++;
            }
            while (value != 0);
            return position;
        }

        public static bool TryDeserialize(ref SequenceReader<byte> reader, out VarLong result)
        {
            if (reader.Remaining < 1)
            {
                result = default;
                return false;
            }

            byte bit;
            long value = 0;
            int offset = 0;
            do
            {
                if (!reader.TryRead(out bit))
                {
                    result = default;
                    return false;
                }

                int segment = bit & SEGMENT_BITS;
                value |= (long)segment << (7 * offset);
                offset++;
                if (offset > 10)
                {
                    result = default;
                    return false;
                }
            } while ((bit & CONTINUE_BIT) != 0);

            result = new VarLong(value);
            return true;
        }

        public static VarLong Deserialize(ref SequenceReader<byte> reader)
        {
            if (reader.Remaining < 1)
            {
                throw new InvalidOperationException("Not enough data to deserialize VarLong");
            }

            byte bit;
            long value = 0;
            int offset = 0;
            do
            {
                if (!reader.TryRead(out bit))
                {
                    throw new InvalidOperationException("Not enough data to deserialize VarLong");
                }

                int segment = bit & SEGMENT_BITS;
                value |= (long)segment << (7 * offset);
                offset++;
                if (offset > 10)
                {
                    throw new InvalidOperationException("VarLong is too large to deserialize");
                }
            } while ((bit & CONTINUE_BIT) != 0);

            return new VarLong(value);
        }

        public static implicit operator VarLong(long value) => new(value);
        public static implicit operator long(VarLong value) => value.Value;
    }
}
