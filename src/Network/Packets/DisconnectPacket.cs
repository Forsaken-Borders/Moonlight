using System.IO;
using Moonlight.Types;

namespace Moonlight.Network.Packets
{
    public class DisconnectPacket : Packet
    {
        public ChatComponent Reason { get; init; }

        public DisconnectPacket(ChatComponent reason)
        {
            Reason = reason;
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculateLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteString(reason.ToJson());
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadNextPacket().Data;
        }

        public override int CalculateLength() => Id.GetVarIntLength() + Reason.ToJson().Length.GetVarIntLength() + Reason.ToJson().Length;
    }
}