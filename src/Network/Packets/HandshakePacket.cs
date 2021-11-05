using System.IO;

namespace Moonlight.Network.Packets
{
    public class HandshakePacket : Packet
    {
        public override int Id { get; init; } = 0x01;
        public int ProtocolVersion { get; init; } = -1;
        public string ServerAddress { get; init; } = "127.0.0.1";
        public ushort ServerPort { get; init; } = 25565;
        public ClientState NextClientState { get; init; } = ClientState.Status;

        public HandshakePacket(byte[] data)
        {
            Data = data;

            using PacketHandler packetHandler = new(data);
            ProtocolVersion = packetHandler.ReadVarInt();
            ServerAddress = packetHandler.ReadString();
            ServerPort = packetHandler.ReadUnsignedShort();
            NextClientState = (ClientState)packetHandler.ReadVarInt();
        }

        public HandshakePacket(int protocolVersion = -1, string serverAddress = "127.0.0.1", ushort serverPort = 25565, ClientState clientState = ClientState.Status) : base()
        {
            ProtocolVersion = protocolVersion;
            ServerAddress = serverAddress;
            ServerPort = serverPort;
            NextClientState = clientState;

            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculateLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteVarInt(protocolVersion);
            packetHandler.WriteString(serverAddress);
            packetHandler.WriteUnsignedShort(serverPort);
            packetHandler.WriteVarInt((int)NextClientState);
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadNextPacket().Data;
        }

        public override int CalculateLength() => Id.GetVarIntLength() + ServerAddress.Length.GetVarIntLength() + ServerAddress.Length + sizeof(ushort) + sizeof(int);
    }

    public enum ClientState
    {
        Status = 1,
        Login = 2
    }
}