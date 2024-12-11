using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Protocol.Net
{
    public record PingPacket : IServerPacket<PingPacket>
    {
        public static VarInt Id { get; } = 0x01;
        public VarLong Payload { get; init; }

        public PingPacket(long payload) => Payload = payload;

        public int CalculateSize() => Id.Length + Payload.Length;

        public int Serialize(Span<byte> target)
        {
            target.Clear();
            int position = Id.Serialize(target);
            position += Payload.Serialize(target[position..]);
            return position;
        }

        public static bool TryDeserialize(ref SequenceReader<byte> reader, [NotNullWhen(true)] out PingPacket? result)
        {
            if (VarInt.TryDeserialize(ref reader, out VarInt id) && id == Id && VarLong.TryDeserialize(ref reader, out VarLong payload))
            {
                result = new PingPacket(payload);
                return true;
            }

            result = default;
            return false;
        }

        public static PingPacket Deserialize(ref SequenceReader<byte> reader) => new(VarLong.Deserialize(ref reader));
    }
}
