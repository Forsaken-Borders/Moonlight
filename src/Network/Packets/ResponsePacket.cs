using System;
using System.IO;
using Moonlight.Types.ServerPing;

namespace Moonlight.Network.Packets
{
    public class ResponsePacket : Packet
    {
        public ServerStatus Payload { get; init; }

        public ResponsePacket(ServerStatus payload)
        {
            ArgumentNullException.ThrowIfNull(payload, nameof(payload));
            Payload = payload;
            UpdateData();
        }

        public override void UpdateData()
        {
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculateLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteString(Payload.ToJson());
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadNextPacket().Data;
        }

        public override int CalculateLength()
        {
            string serverStatusJson = Payload.ToJson();
            return Id.GetVarIntLength() + serverStatusJson.Length.GetVarIntLength() + serverStatusJson.Length;
        }
    }
}