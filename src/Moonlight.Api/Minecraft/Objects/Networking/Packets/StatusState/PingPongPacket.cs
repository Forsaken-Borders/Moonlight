using Moonlight.Api.Minecraft.Abstractions.Networking.Packets;

namespace Moonlight.Api.Minecraft.Objects.Networking.Packets.StatusState
{
    public record PingPongPacket : AbstractPacket
    {
        /// <inheritdoc />
        public override int Id { get; internal set; } = 0x01;

        /// <summary>
        /// The number sent by the client, which is used to determine the server's latency.
        /// </summary>
        public long Payload { get; private set; }

        /// <inheritdoc />
        public PingPongPacket() : base() { }

        /// <inheritdoc />
        public PingPongPacket(int id = 0x01, params byte[] data) : base(id, data) { }

        /// <summary>
        /// Creates a new <see cref="PingPongPacket"/> from the given number.
        /// </summary>
        /// <param name="payload">The number used to determine the server's latency</param>
        public PingPongPacket(long payload)
        {
            ArgumentNullException.ThrowIfNull(payload);
            Payload = payload;
            UpdateData();
        }

        /// <inheritdoc />
        public override void UpdateData()
        {
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculatePacketLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteLong(Payload);
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

            using PacketHandler packetHandler = new(new MemoryStream(Data));
            Payload = packetHandler.ReadLong();
        }

        /// <inheritdoc />
        public override int CalculatePacketLength() => Id.GetVarLength() + Payload.GetVarLength();
    }
}