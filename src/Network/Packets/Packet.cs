using System;
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
    }
}