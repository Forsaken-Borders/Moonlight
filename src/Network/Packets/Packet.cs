using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Moonlight.Network.Packets
{
    public class Packet
    {
        [JsonIgnore]
        public int Id { get; init; }

        [JsonIgnore]
        public byte[] Data { get; set; }

        public Packet() { }

        public Packet(int id) => Id = id;

        public Packet(int id, byte[] data)
        {
            Id = id;
            Data = data;
        }

        public Packet(int id, string data)
        {
            Id = id;
            Data = Encoding.UTF8.GetBytes(data);
        }

        public int CalculateLength() => Id.GetVarIntLength() + (Data?.Length ?? 0);
        public static implicit operator byte[](Packet handshakePacket) => JsonSerializer.SerializeToUtf8Bytes(handshakePacket, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}