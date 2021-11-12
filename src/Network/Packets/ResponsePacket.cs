using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Moonlight.Types;

namespace Moonlight.Network.Packets
{
    public class ResponsePacket : Packet
    {
        public ServerStatus Payload { get; init; }

        public ResponsePacket(ServerStatus payload)
        {
            ArgumentNullException.ThrowIfNull(payload, nameof(payload));
            Payload = payload;
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculateLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteString(JsonSerializer.Serialize(payload, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }));
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadNextPacket().Data;
        }

        public override int CalculateLength()
        {
            string serverStatusJson = Payload.ToJson();
            return Id.GetVarIntLength() + serverStatusJson.Length.GetVarIntLength() + serverStatusJson.Length;
        }
    }
}