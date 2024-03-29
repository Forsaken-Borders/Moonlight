using System;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Protocol.Net
{
    public record StatusRequestPacket : IPacket<StatusRequestPacket>
    {
        public static VarInt Id { get; } = 0x00;

        public int CalculateSize() => Id.Length;

        public int Serialize(Span<byte> target)
        {
            target.Clear();
            int position = Id.Serialize(target);
            return position;
        }

        public static StatusRequestPacket Deserialize(ReadOnlySpan<byte> data, out int offset) => VarInt.Deserialize(data, out offset) == Id
            ? new StatusRequestPacket()
            : throw new InvalidOperationException("Invalid packet id.");
    }
}
