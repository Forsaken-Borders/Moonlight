using System.IO;

namespace Moonlight.Network.Packets
{
    public class HandshakePacket : Packet
    {
        public new int Id { get; } = 0x01;
        public int ProtocolVersion { get; init; } = -1;
        public string ServerAddress { get; init; } = "127.0.0.1";
        public int ServerPort { get; init; } = 25565;
        public ClientState NextClientState { get; init; } = ClientState.Status;

        public HandshakePacket(int id, byte[] data)
        {
            Id = id;
            Data = data;

            PacketHandler packetHandler = new(new MemoryStream(data));
            ProtocolVersion = packetHandler.ReadVarInt();
            ServerAddress = packetHandler.ReadString();
            ServerPort = packetHandler.ReadUnsignedShort();
            NextClientState = (ClientState)packetHandler.ReadVarInt();
        }

        public HandshakePacket(int protocolVersion = -1, string serverAddress = "127.0.0.1", int serverPort = 25565, ClientState clientState = ClientState.Status) : base()
        {
            ProtocolVersion = protocolVersion;
            ServerAddress = serverAddress;
            ServerPort = serverPort;
            NextClientState = clientState;
        }
    }

    public enum ClientState
    {
        Status = 1,
        Login = 2
    }
}