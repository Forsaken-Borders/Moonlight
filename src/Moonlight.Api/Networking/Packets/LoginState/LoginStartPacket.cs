namespace Moonlight.Api.Networking.Packets.LoginState
{
    public class LoginStartPacket : Packet
    {
        public string Username { get; private set; } = null!;

        public LoginStartPacket() { }

        public LoginStartPacket(int id = 0, params byte[] data)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            ArgumentNullException.ThrowIfNull(data, nameof(data));

            Id = id;
            Data = data;
            UpdateProperties();
        }

        public LoginStartPacket(string username)
        {
            ArgumentNullException.ThrowIfNull(username, nameof(username));

            Username = username;
            UpdateData();
        }

        public override void UpdateProperties()
        {
            if (Data == null || Data.Length == 0)
            {
                return;
            }

            using PacketHandler packetHandler = new(new MemoryStream(Data));
            Username = packetHandler.ReadString();
        }

        public override void UpdateData()
        {
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteVarInt(CalculatePacketLength());
            packetHandler.WriteString(Username);
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadPacket().Data;
        }

        public override int CalculatePacketLength() => Id.GetVarLength() + Username.GetVarLength();
    }
}