using System.IO;

namespace Moonlight.Network.Packets
{
    public class PingPongPacket : Packet
    {
        public new int Id { get; init; } = 0x01;
        public long Payload { get; init; }

        public PingPongPacket(int id, byte[] data)
        {
            Id = id;
            Data = data;
            using PacketHandler packetHandler = new(data);
            Payload = packetHandler.ReadLong();
        }

        public PingPongPacket(long payload)
        {
            Payload = payload;
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculateLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteLong(Payload);
            Data = packetHandler.ReadNextPacket().Data;
        }
    }
}