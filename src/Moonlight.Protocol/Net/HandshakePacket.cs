using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Protocol.Net
{
    public record HandshakePacket : IServerPacket<HandshakePacket>
    {
        public static VarInt Id { get; } = 0x00;
        public VarInt ProtocolVersion { get; init; }
        public string ServerAddress { get; init; }
        public ushort ServerPort { get; init; }
        public VarInt NextState { get; init; }

        public HandshakePacket(VarInt protocolVersion, string serverAddress, ushort serverPort, VarInt nextState)
        {
            try
            {
                Dns.GetHostEntry(serverAddress);
            }
            catch (Exception error)
            {
                throw new ArgumentException("Invalid server address.", nameof(serverAddress), error);
            }

            ProtocolVersion = protocolVersion;
            ServerAddress = serverAddress;
            ServerPort = serverPort;
            NextState = nextState;
        }

        public int CalculateSize() => Id.Length + ProtocolVersion.Length + Encoding.UTF8.GetByteCount(ServerAddress) + sizeof(ushort) + NextState.Length;

        public int Serialize(Span<byte> target)
        {
            target.Clear();
            int position = Id.Serialize(target);
            position += ProtocolVersion.Serialize(target[position..]);
            position += Encoding.UTF8.GetBytes(ServerAddress, target[position..]);
            if (!BitConverter.TryWriteBytes(target[position..], ServerPort))
            {
                throw new InvalidOperationException("Unable to write server port.");
            }

            position += sizeof(ushort);
            position += NextState.Serialize(target[position..]);
            return position;
        }

        public static bool TryDeserialize(ref SequenceReader<byte> reader, [NotNullWhen(true)] out HandshakePacket? result)
        {
            if (!VarInt.TryDeserialize(ref reader, out VarInt id) || id != Id)
            {
                result = default;
                return false;
            }

            if (!VarInt.TryDeserialize(ref reader, out VarInt protocolVersion))
            {
                result = default;
                return false;
            }

            SequencePosition? addressEndPosition = reader.UnreadSequence.PositionOf((byte)0);
            if (addressEndPosition is null)
            {
                result = default;
                return false;
            }

            string serverAddress = Encoding.UTF8.GetString(reader.UnreadSequence.Slice(0, addressEndPosition.Value));
            reader.Advance(reader.UnreadSequence.GetOffset(addressEndPosition.Value) + 1);

            if (!reader.TryReadLittleEndian(out short serverPort))
            {
                result = default;
                return false;
            }

            if (!VarInt.TryDeserialize(ref reader, out VarInt nextState))
            {
                result = default;
                return false;
            }

            result = new HandshakePacket(protocolVersion, serverAddress, Unsafe.As<short, ushort>(ref serverPort), nextState);
            return true;
        }

        [SuppressMessage("Roslyn", "IDE0046", Justification = "Ternary rabbit hole.")]
        public static HandshakePacket Deserialize(ref SequenceReader<byte> reader)
        {
            if (!VarInt.TryDeserialize(ref reader, out VarInt protocolVersion))
            {
                throw new InvalidOperationException("Unable to read protocol version.");
            }

            if (!VarInt.TryDeserialize(ref reader, out VarInt addressLength))
            {
                throw new InvalidOperationException("Unable to read server address length.");
            }

            if (!reader.TryReadExact(addressLength, out ReadOnlySequence<byte> addressSequence))
            {
                throw new InvalidOperationException("Unable to read server address.");
            }

            string serverAddress = Encoding.UTF8.GetString(addressSequence);
            if (!reader.TryReadBigEndian(out short serverPort))
            {
                throw new InvalidOperationException("Unable to read server port.");
            }

            if (!VarInt.TryDeserialize(ref reader, out VarInt nextState))
            {
                throw new InvalidOperationException("Unable to read next state.");
            }

            return new HandshakePacket(protocolVersion, serverAddress, (ushort)serverPort, nextState);
        }
    }
}
