using System;
using System.IO;
using Moonlight.Types;

namespace Moonlight.Network.Packets
{
    public class LoginSuccessPacket : Packet
    {
        public override int Id { get; init; } = 0x02;
        public Guid UUID { get; init; }
        public string Username { get; init; }

        public LoginSuccessPacket(MojangSessionServerResponse mojangSessionServerResponse)
        {
            ArgumentNullException.ThrowIfNull(mojangSessionServerResponse, nameof(mojangSessionServerResponse));
            UUID = Guid.Parse(mojangSessionServerResponse.Id);
            Username = mojangSessionServerResponse.Name;
            UpdateData();
        }

        public override void UpdateData()
        {
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculateLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteString(UUID.ToString());
            packetHandler.WriteString(Username);
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadNextPacket().Data;
        }
    }
}