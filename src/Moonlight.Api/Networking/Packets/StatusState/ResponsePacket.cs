using System.Text.Json;
using Moonlight.Api;
using Moonlight.Api.Networking;
using Moonlight.Api.Networking.Packets;
using Moonlight.Api.Types.ServerPing;

namespace Moonlight.Network.Packets.StatusState
{
    public class ResponsePacket : Packet
    {
        public ServerStatus Payload { get; private set; } = null!;

        public ResponsePacket() { }

        public ResponsePacket(int id = 0, params byte[] data)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            ArgumentNullException.ThrowIfNull(data, nameof(data));

            Id = id;
            Data = data;
            UpdateProperties();
        }

        public ResponsePacket(ServerStatus payload)
        {
            ArgumentNullException.ThrowIfNull(payload, nameof(payload));
            Payload = payload;
            UpdateData();
        }

        public override void UpdateData()
        {
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculatePacketLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteString(Payload.ToJson());
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadPacket().Data;
        }

        public override void UpdateProperties()
        {
            if (Data == null || Data.Length == 0)
            {
                return;
            }

            Payload = JsonSerializer.Deserialize<ServerStatus>(Data) ?? throw new ArgumentException("Failed to deserialize to a ServerStatus; invalid json provided.");
        }
        public override int CalculatePacketLength() => Id.GetVarLength() + Payload.ToJson().GetVarLength();
    }
}