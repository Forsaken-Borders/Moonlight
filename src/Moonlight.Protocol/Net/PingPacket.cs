using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Protocol.Net
{
    public record PingPacket : IServerPacket<PingPacket>
    {
        public static VarInt Id { get; } = 0x01;
        public required VarLong Payload { get; init; }

        public int CalculateSize() => Id.Length + Payload.Length;

        public int Serialize(Span<byte> target)
        {
            int position = Id.Serialize(target);
            position += Payload.Serialize(target[position..]);
            return position;
        }

        public static bool TryDeserialize(ref SequenceReader<byte> reader, [NotNullWhen(true)] out PingPacket? result)
        {
            if (VarLong.TryDeserialize(ref reader, out VarLong payload))
            {
                result = new PingPacket()
                {
                    Payload = payload
                };

                return true;
            }

            result = default;
            return false;
        }

        public static PingPacket Deserialize(ref SequenceReader<byte> reader) => new()
        {
            Payload = VarLong.Deserialize(ref reader)
        };
    }
}
