namespace Moonlight.Api.Networking.Packets.StatusState
{
    public class PingPongPacket : Packet
    {
        public new int Id { get; init; } = 0x01;
        public long Payload { get; private set; }

        public PingPongPacket() { }

        public PingPongPacket(int id = 0x01, params byte[] data)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            ArgumentNullException.ThrowIfNull(data, nameof(data));

            Id = id;
            Data = data;
            UpdateProperties();
        }

        public PingPongPacket(long payload)
        {
            ArgumentNullException.ThrowIfNull(payload);
            Payload = payload;
            UpdateData();
        }

        public override void UpdateData()
        {
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculatePacketLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteLong(Payload);
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadPacket().Data;
        }

        public override void UpdateProperties()
        {
            if (Data == null || Data.Length == 0)
            {
                return;
            }

            using PacketHandler packetHandler = new(new MemoryStream(Data));
            Payload = packetHandler.ReadLong();
        }

        public override int CalculatePacketLength() => Id.GetVarLength() + Payload.GetVarLength();
    }
}