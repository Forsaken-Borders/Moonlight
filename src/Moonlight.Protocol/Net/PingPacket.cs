using System;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Protocol.Net
{
    public record PingPacket : IPacket<PingPacket>
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

        public static PingPacket Deserialize(ReadOnlySpan<byte> data, out int offset) => VarInt.Deserialize(data, out _) != Id
            ? throw new InvalidOperationException("Invalid packet id.")
            : new PingPacket(VarLong.Deserialize(data[Id.Length..], out offset));
    }
}
