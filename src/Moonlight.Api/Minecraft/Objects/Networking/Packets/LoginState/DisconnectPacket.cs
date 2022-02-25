using Moonlight.Api.Minecraft.Abstractions.Networking.Packets;
using Moonlight.Api.Minecraft.Objects.Chat;
using System.Text.Json;

namespace Moonlight.Api.Minecraft.Objects.Networking.Packets.LoginState
{
    public record DisconnectPacket : AbstractPacket
    {
        /// <summary>
        /// Why the client was disconnected.
        /// </summary>
        public ChatComponent? Reason { get; private set; }

        /// <inheritdoc />
        public DisconnectPacket() : base() { }

        /// <inheritdoc />
        public DisconnectPacket(int id = 0x00, params byte[] data) : base(id, data) { }

        /// <summary>
        /// Creates a new disconnect packet with the specified reason.
        /// </summary>
        /// <param name="reason">Why the client was disconnected.</param>
        public DisconnectPacket(ChatComponent reason)
        {
            ArgumentNullException.ThrowIfNull(reason, nameof(reason));
            Reason = reason;
            UpdateData();
        }

        /// <inheritdoc />
        public override int CalculatePacketLength() => Id.GetVarLength() + (Reason?.ToJson().GetVarLength() ?? 0);

        /// <inheritdoc />
        public override void UpdateData()
        {
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculatePacketLength());
            packetHandler.WriteVarInt(Id);
            if (Reason != null)
            {
                packetHandler.WriteString(Reason.ToJson());
            }
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

            Reason = JsonSerializer.Deserialize<ChatComponent>(Data) ?? throw new ArgumentException("Failed to deserialize to a ChatComponent; invalid json provided.");
        }
    }
}