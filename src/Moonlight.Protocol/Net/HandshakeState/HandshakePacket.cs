using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Protocol.Net.HandshakeState
{
    public record HandshakePacket : IPacket<HandshakePacket>
    {
        public static VarInt Id { get; } = 0x00;
        public required VarInt ProtocolVersion { get; init; }
        public required string ServerAddress { get; init; }
        public required ushort ServerPort { get; init; }
        public required VarInt NextState { get; init; }

        public static int CalculateSize(HandshakePacket packet) => packet.ProtocolVersion.Length + Encoding.UTF8.GetByteCount(packet.ServerAddress) + sizeof(ushort) + packet.NextState.Length;

        public static int Serialize(HandshakePacket packet, Span<byte> target)
        {
            target.Clear();
            int position = VarInt.Serialize(Id, target);
            position += VarInt.Serialize(packet.ProtocolVersion, target[position..]);
            position += Encoding.UTF8.GetBytes(packet.ServerAddress, target[position..]);
            if (!BitConverter.TryWriteBytes(target[position..], packet.ServerPort))
            {
                throw new InvalidOperationException("Unable to write server port.");
            }

            position += sizeof(ushort);
            position += VarInt.Serialize(packet.NextState, target[position..]);
            return position;
        }

        public static bool TryDeserialize(ref SequenceReader<byte> reader, [NotNullWhen(true)] out HandshakePacket? result)
        {
            if (!reader.TryReadVarInt(out VarInt protocolVersion)
                || !reader.TryReadString(out string? serverAddress)
                || !reader.TryReadUnsignedShort(out ushort serverPort)
                || !reader.TryReadVarInt(out VarInt nextState))
            {
                result = default;
                return false;
            }

            result = new HandshakePacket()
            {
                ProtocolVersion = protocolVersion,
                ServerAddress = serverAddress,
                ServerPort = serverPort,
                NextState = nextState
            };

            return true;
        }

        [SuppressMessage("Roslyn", "IDE0046", Justification = "Ternary rabbit hole.")]
        public static HandshakePacket Deserialize(ref SequenceReader<byte> reader)
        {
            if (!reader.TryReadVarInt(out VarInt protocolVersion))
            {
                throw new ProtocolViolationException("Unable to read protocol version.");
            }

            if (!reader.TryReadString(out string? serverAddress))
            {
                throw new ProtocolViolationException("Unable to read server address.");
            }

            if (!reader.TryReadUnsignedShort(out ushort serverPort))
            {
                throw new ProtocolViolationException("Unable to read server port.");
            }

            if (!reader.TryReadVarInt(out VarInt nextState))
            {
                throw new ProtocolViolationException("Unable to read next state.");
            }

            return new HandshakePacket()
            {
                ProtocolVersion = protocolVersion,
                ServerAddress = serverAddress,
                ServerPort = serverPort,
                NextState = nextState
            };
        }
    }
}
