using System.IO;

namespace Moonlight.Network.Packets
{
    public class LoginStartPacket : Packet
    {
        public string Username { get; init; }

        public LoginStartPacket(byte[] data)
        {
            Data = data;
            using PacketHandler packetHandler = new(data);
            Username = packetHandler.ReadString();
        }

        public LoginStartPacket(string username)
        {
            Username = username;
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculateLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteString(username);
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadNextPacket().Data;
        }

        public override int CalculateLength() => Id.GetVarIntLength() + Username.Length.GetVarIntLength() + Username.Length;
    }
}