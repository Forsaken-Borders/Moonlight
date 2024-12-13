using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Protocol.Net.StatusState
{
    public record StatusRequestPacket : IStatusPacket<StatusRequestPacket>
    {
        public static VarInt Id { get; } = 0x00;

        public int CalculateSize() => Id.Length;

        public int Serialize(Span<byte> target)
        {
            int position = Id.Serialize(target);
            return position;
        }

        public static bool TryDeserialize(ref SequenceReader<byte> reader, [NotNullWhen(true)] out StatusRequestPacket? result)
        {
            result = new StatusRequestPacket();
            return true;
        }

        public static StatusRequestPacket Deserialize(ref SequenceReader<byte> reader) => new();
    }
}
