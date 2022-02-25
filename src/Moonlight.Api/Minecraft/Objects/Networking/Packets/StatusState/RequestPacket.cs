using Moonlight.Api.Minecraft.Abstractions.Networking.Packets;

namespace Moonlight.Api.Minecraft.Objects.Networking.Packets.StatusState
{
    public record RequestPacket : AbstractPacket
    {
        /// <inheritdoc />
        public RequestPacket() : base() { }

        /// <inheritdoc />
        public RequestPacket(int id = 0x00, params byte[] data) : base(id, data) { }

        /// <inheritdoc />
        public override int CalculatePacketLength() => Id.GetVarLength();

        /// <inheritdoc />
        public override void UpdateData() => throw new NotSupportedException();

        /// <inheritdoc />
        public override void UpdateProperties() => throw new NotSupportedException();
    }
}