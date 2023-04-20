using System;

namespace Moonlight.Protocol.VariableTypes
{
    public readonly record struct VarInt : ISpanSerializable<VarInt>
    {
        private const int SEGMENT_BITS = 0x7F;
        private const int CONTINUE_BIT = 0x80;

        public int Value { get; init; }
        public int Length { get; init; }

        public VarInt(int value)
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
            int value = Value;
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

        public static VarInt Deserialize(ReadOnlySpan<byte> data, out int offset)
        {
            offset = 0;
            int value = 0;
            byte bit;
            do
            {
                bit = data[offset];
                value |= (bit & SEGMENT_BITS) << (7 * offset);
                offset++;
            } while ((bit & CONTINUE_BIT) == CONTINUE_BIT);

            return new VarInt(value);
        }

        public static implicit operator VarInt(int value) => new(value);
        public static implicit operator int(VarInt value) => value.Value;
    }
}
