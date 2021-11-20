using System.IO;
using Moonlight.Types.Chat;

namespace Moonlight.Network.Packets
{
    public class DisconnectPacket : Packet
    {
        public ChatComponent Reason { get; init; }

        public DisconnectPacket(ChatComponent reason)
        {
            Reason = reason ?? "&cDisconnected for an unknown reason.";
            reason.Text = reason.ToString();
            UpdateData();
        }

        public override void UpdateData()
        {
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculateLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteString(Reason.ToJson());
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadNextPacket().Data;
        }

        public override int CalculateLength()
        {
            string reasonJson = Reason.ToJson();
            return Id.GetVarIntLength() + reasonJson.Length.GetVarIntLength() + reasonJson.Length;
        }
    }
}