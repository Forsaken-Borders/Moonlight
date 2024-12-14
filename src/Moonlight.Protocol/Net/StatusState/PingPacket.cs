using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Protocol.Net.StatusState
{
    public record PingPacket : IStatusPacket<PingPacket>
    {
        public static VarInt Id { get; } = 0x01;
        public required VarLong Payload { get; init; }

        public static int CalculateSize(PingPacket packet) => packet.Payload.Length;

        public static int Serialize(PingPacket packet, Span<byte> target)
        {
            int position = VarInt.Serialize(Id, target);
            position += VarLong.Serialize(packet.Payload, target[position..]);
            return position;
        }

        public static bool TryDeserialize(ref SequenceReader<byte> reader, [NotNullWhen(true)] out PingPacket? result)
        {
            if (!reader.TryReadVarLong(out VarLong payload))
            {
                result = default;
                return false;
            }

            result = new PingPacket()
            {
                Payload = payload
            };

            return true;
        }

        public static PingPacket Deserialize(ref SequenceReader<byte> reader) => !TryDeserialize(ref reader, out PingPacket? result)
            ? throw new ProtocolViolationException("Not enough data to deserialize PingPacket.")
            : result;
    }
}
