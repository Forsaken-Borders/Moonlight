using System.IO;
using Moonlight.Types;

namespace Moonlight.Network.Packets
{
    public class DisconnectPacket : Packet
    {
        public ChatComponent Reason { get; init; }

        public DisconnectPacket(ChatComponent reason)
        {
            Reason = reason ?? "Disconnected for an unknown reason.";
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculateLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteString(reason.ToJson());
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