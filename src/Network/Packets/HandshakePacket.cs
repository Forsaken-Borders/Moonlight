using System.IO;
using System.Threading.Tasks;

namespace Moonlight.Network.Packets
{
    public class HandshakePacket : Packet
    {
        public new int Id { get; } = 0x01;
        public int ProtocolVersion { get; init; } = -1;
        public string ServerAddress { get; init; } = "127.0.0.1";
        public int ServerPort { get; init; } = 25565;
        public ClientState NextClientState { get; init; } = ClientState.Status;

        public HandshakePacket(int protocolVersion = -1, string serverAddress = "127.0.0.1", int serverPort = 25565, ClientState clientState = ClientState.Status) : base()
        {
            ProtocolVersion = protocolVersion;
            ServerAddress = serverAddress;
            ServerPort = serverPort;
            NextClientState = clientState;
        }

        public static async Task<HandshakePacket> Create(byte[] data)
        {
            PacketHandler packetHandler = new(new MemoryStream(data));
            int protocolVersion = await packetHandler.ReadVarIntAsync();
            string serverAddress = await packetHandler.ReadStringAsync();
            ushort serverPort = await packetHandler.ReadUnsignedShortAsync();
            ClientState nextClientState = (ClientState)await packetHandler.ReadVarIntAsync();

            return new()
            {
                ProtocolVersion = protocolVersion,
                ServerAddress = serverAddress,
                ServerPort = serverPort,
                NextClientState = nextClientState
            };
        }
    }

    public enum ClientState
    {
        Status = 1,
        Login = 2
    }
}