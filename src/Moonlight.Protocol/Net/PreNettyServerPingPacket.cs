using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Protocol.Net
{
    public record PreNettyServerPingPacket : IServerPacket<PreNettyServerPingPacket>
    {
        public static VarInt Id => 0xFE;

        public int CalculateSize() => 0;

        public int Serialize(Span<byte> target) => 0;

        public static PreNettyServerPingPacket Deserialize(ref SequenceReader<byte> reader) => new();

        public static bool TryDeserialize(ref SequenceReader<byte> reader, [NotNullWhen(true)] out PreNettyServerPingPacket? result)
        {
            result = new PreNettyServerPingPacket();
            return true;
        }
    }
}
