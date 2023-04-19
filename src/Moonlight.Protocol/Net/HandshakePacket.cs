using System;
using System.Net;
using System.Text;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Protocol.Net
{
    public record HandshakePacket : IPacket<HandshakePacket>
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

        public static HandshakePacket Deserialize(Span<byte> data)
        {
            if (VarInt.Deserialize(data) != Id)
            {
                throw new InvalidOperationException("Invalid packet id.");
            }

            int position = Id.Length;
            VarInt protocolVersion = VarInt.Deserialize(data[position..]);
            position += protocolVersion.Length;

            int addressLength = data[position..].IndexOf((byte)0);
            string serverAddress = Encoding.UTF8.GetString(data[position..(position + addressLength)]);
            position += addressLength + 1;

            ushort serverPort = BitConverter.ToUInt16(data[position..]);
            position += sizeof(ushort);

            VarInt nextState = VarInt.Deserialize(data[position..]);
            return new HandshakePacket(protocolVersion, serverAddress, serverPort, nextState);
        }
    }
}
