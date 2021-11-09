using System;
using System.IO;

namespace Moonlight.Network.Packets
{
    public class PingPongPacket : Packet
    {
        public new int Id { get; init; } = 0x01;
        public long Payload { get; init; }

        public PingPongPacket(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data, nameof(data));
            Data = data;
            using PacketHandler packetHandler = new(data);
            Payload = packetHandler.ReadLong();
        }

        public PingPongPacket(long payload)
        {
            ArgumentNullException.ThrowIfNull(payload, nameof(payload));
            Payload = payload;
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculateLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteLong(Payload);
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadNextPacket().Data;
        }

        public override int CalculateLength() => Id.GetVarIntLength() + Payload.GetVarLongLength();
    }
}