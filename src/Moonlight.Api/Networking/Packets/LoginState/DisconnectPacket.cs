using Moonlight.Api.Types.Chat;

namespace Moonlight.Api.Networking.Packets.LoginState
{
    public class DisconnectPacket : Packet
    {
        public ChatComponent Reason { get; private set; } = null!;

        public DisconnectPacket() { }

        public DisconnectPacket(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);
            Data = data;
            UpdateProperties();
        }

        public DisconnectPacket(ChatComponent reason)
        {
            ArgumentNullException.ThrowIfNull(reason);

            reason.Text = reason.ToString(); // Put everything into one string, in case older or future clients drop support for certain properties (or never had them in the first place)
            Reason = reason ?? "&cDisconnected for an unknown reason.";
            UpdateData();
        }

        public override void UpdateData()
        {
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculatePacketLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteString(Reason.ToJson());
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
            Reason = new(packetHandler.ReadString());
        }

        public override int CalculatePacketLength() => Id.GetVarLength() + Reason.ToJson().GetVarLength();
    }
}