using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;

namespace Moonlight.Api.Networking.Packets.LoginState
{
    public class EncryptionResponsePacket : Packet
    {
        public override int Id { get; set; } = 0x01;
        public byte[] SharedSecret { get; private set; } = null!;
        public byte[] VerifyToken { get; private set; } = null!;
        public AsymmetricKeyParameter PrivateKey { get; set; } = null!;

        public EncryptionResponsePacket() { }

        public EncryptionResponsePacket(byte[] sharedSecret, byte[] verifyToken, AsymmetricKeyParameter privateKey)
        {
            ArgumentNullException.ThrowIfNull(sharedSecret, nameof(sharedSecret));
            ArgumentNullException.ThrowIfNull(verifyToken, nameof(verifyToken));
            ArgumentNullException.ThrowIfNull(privateKey, nameof(privateKey));

            SharedSecret = sharedSecret;
            VerifyToken = verifyToken;
            PrivateKey = privateKey;
            UpdateData();
        }

        public EncryptionResponsePacket(byte[] data, AsymmetricKeyParameter privateKey)
        {
            ArgumentNullException.ThrowIfNull(data, nameof(data));
            ArgumentNullException.ThrowIfNull(privateKey, nameof(privateKey));

            Data = data;
            PrivateKey = privateKey;
            UpdateProperties();
        }

        public override void UpdateData()
        {
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteVarInt(CalculatePacketLength());
            packetHandler.WriteVarInt(SharedSecret.Length);
            packetHandler.WriteUnsignedByteArray(SharedSecret);
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
            else if (PrivateKey == null)
            {
                throw new InvalidOperationException($"Property ({nameof(PrivateKey)}) cannot be null.");
            }

            using PacketHandler packetHandler = new(new MemoryStream(Data));
            SharedSecret = packetHandler.ReadByteArray(packetHandler.ReadVarInt());
            VerifyToken = packetHandler.ReadByteArray(packetHandler.ReadVarInt());

            IAsymmetricBlockCipher cipher = new Pkcs1Encoding(new RsaEngine());
            cipher.Init(false, PrivateKey);

            SharedSecret = cipher.ProcessBlock(SharedSecret, 0, cipher.GetInputBlockSize());
            VerifyToken = cipher.ProcessBlock(VerifyToken, 0, cipher.GetInputBlockSize());
        }

        public override int CalculatePacketLength() => Id.GetVarLength() + SharedSecret.GetVarLength() + VerifyToken.GetVarLength();
    }
}