using Moonlight.Api.Minecraft.Abstractions.Networking.Packets;

namespace Moonlight.Api.Minecraft.Objects.Networking.Packets
{
    /// <summary>
    /// Represents a Minecraft packet.
    /// </summary>
    public record Packet : AbstractPacket
    {
        /// <inheritdoc />
        public Packet() : base() { }

        /// <inheritdoc />
        public Packet(int id = 0, params byte[] data) : base(id, data) { }

        /// <inheritdoc />
        public override int CalculatePacketLength() => Id.GetVarLength() + (Data?.GetVarLength() ?? 0);

        /// <inheritdoc />
        public override void UpdateData() { }

        /// <inheritdoc />
        public override void UpdateProperties() { }
    }
}