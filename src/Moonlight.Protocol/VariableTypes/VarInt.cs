using System;

namespace Moonlight.Protocol.VariableTypes
{
    public readonly record struct VarInt : ISpanSerializable<VarInt>
    {
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
                byte bit = (byte)(value & 127);
                value >>= 7;
                if (value != 0)
                {
                    bit |= 128;
                }

                target[position] = bit;
                position++;
            }
            while (value != 0);
            return position;
        }

        public static VarInt Deserialize(Span<byte> data)
        {
            int value = 0;
            int position = 0;
            byte bit;
            do
            {
                bit = data[position];
                value |= (bit & 127) << (7 * position);
                position++;
            } while ((bit & 128) == 128);

            return new VarInt(value);
        }

        public static implicit operator VarInt(int value) => new(value);
        public static implicit operator int(VarInt value) => value.Value;
    }
}
