using Moonlight.Api.Mojang;

namespace Moonlight.Api.Networking.Packets.LoginState
{
    public class LoginSuccessPacket : Packet
    {
        public override int Id { get; set; } = 0x02;
        public Guid UUID { get; private set; }
        public string Username { get; private set; } = null!;

        public LoginSuccessPacket() { }

        public LoginSuccessPacket(int id = 0, params byte[] data)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            ArgumentNullException.ThrowIfNull(data, nameof(data));

            Id = id;
            Data = data;
            UpdateProperties();
        }

        public LoginSuccessPacket(SessionServerResponse mojangSessionServerResponse)
        {
            ArgumentNullException.ThrowIfNull(mojangSessionServerResponse, nameof(mojangSessionServerResponse));
            UUID = Guid.Parse(mojangSessionServerResponse.Id);
            Username = mojangSessionServerResponse.Name;
            UpdateData();
        }

        public override void UpdateData()
        {
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculatePacketLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteString(UUID.ToString());
            packetHandler.WriteString(Username);
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
            UUID = Guid.Parse(packetHandler.ReadString());
            Username = packetHandler.ReadString();
        }

        public override int CalculatePacketLength() => Id.GetVarLength() + UUID.ToString().GetVarLength() + Username.GetVarLength();
    }
}