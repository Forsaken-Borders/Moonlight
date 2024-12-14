using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Protocol.Net
{
    public record UnknownPacket : IServerPacket<UnknownPacket>
    {
        public static VarInt Id { get; } = -1;
        public VarInt ReceivedId { get; init; }
        public ReadOnlyMemory<byte> Data { get; init; }

        public UnknownPacket(VarInt id, ReadOnlyMemory<byte> data)
        {
            ReceivedId = id;
            Data = data;
        }

        public static int CalculateSize(UnknownPacket packet) => Id.Length + packet.Data.Length;

        public static int Serialize(UnknownPacket packet, Span<byte> target)
        {
            int position = VarInt.Serialize(Id, target);
            packet.Data.Span.CopyTo(target[position..]);
            return position + packet.Data.Length;
        }

        public static bool TryDeserialize(ref SequenceReader<byte> reader, [NotNullWhen(true)] out UnknownPacket? result)
        {
            if (!reader.TryReadVarInt(out VarInt id))
            {
                result = default;
                return false;
            }

            ReadOnlyMemory<byte> data = reader.UnreadSequence.ToArray();
            result = new UnknownPacket(id, data);
            reader.Advance(data.Length);
            return true;
        }

        public static UnknownPacket Deserialize(ref SequenceReader<byte> reader)
        {
            VarInt id = VarInt.Deserialize(ref reader);
            ReadOnlyMemory<byte> data = reader.UnreadSequence.ToArray();
            reader.Advance(data.Length);
            return new UnknownPacket(id, data);
        }
    }
}
