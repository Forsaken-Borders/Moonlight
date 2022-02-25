using Moonlight.Api.Minecraft.Abstractions.Networking.Packets;
using Moonlight.Api.Minecraft.Objects.Models.ServerPing;
using System.Text.Json;

namespace Moonlight.Api.Minecraft.Objects.Networking.Packets.StatusState
{
    public record ResponsePacket : AbstractPacket
    {
        /// <summary>
        /// The server status to send to the client.
        /// </summary>
        public ServerStatus Payload { get; set; } = null!;

        /// <inheritdoc />
        public ResponsePacket() : base() { }

        /// <inheritdoc />
        public ResponsePacket(int id = 0x00, params byte[] data) : base(id, data) { }

        /// <summary>
        /// Creates a new response packet from the given server status.
        /// </summary>
        /// <param name="payload">The server status to return to the client.</param>
        public ResponsePacket(ServerStatus payload)
        {
            ArgumentNullException.ThrowIfNull(payload, nameof(payload));
            Payload = payload;
            UpdateData();
        }

        /// <inheritdoc />
        public override void UpdateData()
        {
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculatePacketLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteString(Payload.ToJson());
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadPacket().Data;
        }

        /// <inheritdoc />
        public override void UpdateProperties()
        {
            if (Data == null || Data.Length == 0)
            {
                return;
            }

            Payload = JsonSerializer.Deserialize<ServerStatus>(Data) ?? throw new ArgumentException("Failed to deserialize to a ServerStatus; invalid json provided.");
        }

        /// <inheritdoc />
        public override int CalculatePacketLength() => Id.GetVarLength() + Payload.ToJson().GetVarLength();
    }
}