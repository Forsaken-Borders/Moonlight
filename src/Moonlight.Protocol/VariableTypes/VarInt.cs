using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Moonlight.Protocol.VariableTypes
{
    public readonly record struct VarInt : ISpanSerializable<VarInt>
    {
        public const int MaxValue = int.MaxValue;
        public const int MinValue = int.MinValue;
        public const int MaxLength = 5;
        public const int MinLength = 1;

        private const int SEGMENT_BITS = 127;
        private const int CONTINUE_BIT = 128;

        public required int Value { get; init; }
        public required int Length { get; init; }

        [SetsRequiredMembers]
        public VarInt(int value)
        {
            Value = value;

            uint unsignedValue = (uint)value;
            do
            {
                unsignedValue >>= 7;
                Length++;
            } while (unsignedValue != 0);
        }

        public static int CalculateSize(VarInt varInt) => varInt.Length;

        public static int Serialize(VarInt varInt, Span<byte> target)
        {
            uint value = (uint)varInt.Value;
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
            } while (value != 0);
            return position;
        }

        public static bool TryDeserialize(ref SequenceReader<byte> reader, out VarInt result)
        {
            if (reader.Remaining < 1)
            {
                result = default;
                return false;
            }

            byte bit;
            int value = 0;
            int offset = 0;
            do
            {
                if (!reader.TryRead(out bit))
                {
                    result = default;
                    return false;
                }

                value |= (bit & SEGMENT_BITS) << (7 * offset);
                offset++;
                if (offset > 5)
                {
                    throw new InvalidOperationException("VarInt is too big.");
                }

            } while ((bit & CONTINUE_BIT) == CONTINUE_BIT);

            result = new VarInt()
            {
                Value = value,
                Length = offset
            };

            return true;
        }

        public static VarInt Deserialize(ref SequenceReader<byte> reader)
            => !TryDeserialize(ref reader, out VarInt result) ? throw new InvalidOperationException("Not enough data.") : result;

        public static implicit operator VarInt(int value) => new(value);
        public static implicit operator int(VarInt value) => value.Value;
        public override string ToString() => Value.ToString();
    }
}
