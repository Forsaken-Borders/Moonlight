namespace Moonlight.Api.Networking.Packets.LoginState
{
    public class EncryptionRequestPacket : Packet
    {
        public override int Id => 0x01;
        public static readonly string StaticServerId = new(Enumerable.Range(0, 10).Select(n => (char)Random.Shared.Next('A', 'z' + 1)).ToArray());

        public string ServerId { get; private set; } = StaticServerId;
        public byte[] PublicKey { get; private set; } = null!;
        public byte[] VerifyToken { get; private set; } = null!;

        public EncryptionRequestPacket() { }

        public EncryptionRequestPacket(int id = 0, params byte[] data)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            ArgumentNullException.ThrowIfNull(data, nameof(data));

            Id = id;
            Data = data;
            UpdateProperties();
        }

        public EncryptionRequestPacket(byte[] publicKey, byte[] verifyToken)
        {
            ArgumentNullException.ThrowIfNull(publicKey, nameof(publicKey));
            ArgumentNullException.ThrowIfNull(verifyToken, nameof(verifyToken));

            PublicKey = publicKey;
            VerifyToken = verifyToken;
            UpdateData();
        }

        public override void UpdateData()
        {
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculatePacketLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteString(ServerId);
            packetHandler.WriteVarInt(PublicKey.Length);
            packetHandler.WriteUnsignedByteArray(PublicKey);
            packetHandler.WriteVarInt(VerifyToken.Length);
            packetHandler.WriteUnsignedByteArray(VerifyToken);
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
            ServerId = packetHandler.ReadString();
            PublicKey = packetHandler.ReadByteArray(packetHandler.ReadVarInt());
            VerifyToken = packetHandler.ReadByteArray(packetHandler.ReadVarInt());
        }

        public override int CalculatePacketLength() => Id.GetVarLength() + ServerId.GetVarLength() + PublicKey.GetVarLength() + VerifyToken.GetVarLength();
    }
}