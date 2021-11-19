using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Moonlight.Network.Packets
{
    public class Packet
    {
        [JsonIgnore]
        public virtual int Id { get; init; }

        [JsonIgnore]
        public byte[] Data { get; set; }

        public Packet() { }

        public Packet(int id) => Id = id;

        public Packet(int id, byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data, nameof(data));

            Id = id;
            Data = data;
        }

        public Packet(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data, nameof(data));
            Data = data;
        }

        public virtual int CalculateLength() => Id.GetVarIntLength() + (Data?.Length ?? 0);
        public virtual void UpdateData()
        {
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculateLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadNextPacket().Data;
        }

        public static bool operator ==(Packet packet1, Packet packet2)
        {
            if (packet1.Id == 0 && packet1.Data == null)
            {
                if (packet2.Id == 0 && packet2.Data == null)
                {
                    return true;
                }
            }

            return packet1.Equals(packet2);
        }

        public static bool operator !=(Packet packet1, Packet packet2) => !(packet1 == packet2);
        public override bool Equals(object obj) => obj is Packet packet && Id == packet.Id && EqualityComparer<byte[]>.Default.Equals(Data, packet.Data);
        public override int GetHashCode() => HashCode.Combine(Id, Data);
    }
}