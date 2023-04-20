using System;

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

        public static VarLong Deserialize(ReadOnlySpan<byte> data, out int offset)
        {
            offset = 0;
            long result = 0;
            byte read;
            do
            {
                read = data[offset];
                int value = read & SEGMENT_BITS;
                result |= (long)value << (7 * offset);

                offset++;
                if (offset > 10)
                {
                    throw new InvalidOperationException("VarLong is too big.");
                }
            } while ((read & CONTINUE_BIT) != 0);

            return result;
        }

        public static implicit operator VarLong(long value) => new(value);
        public static implicit operator long(VarLong value) => value.Value;
    }
}
