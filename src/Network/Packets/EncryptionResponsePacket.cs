using System.IO;

namespace Moonlight.Network.Packets
{
    public class EncryptionResponsePacket : Packet
    {
        public override int Id { get; init; } = 0x01;
        public byte[] SharedSecret { get; init; }
        public byte[] VerifyToken { get; init; }

        public EncryptionResponsePacket(byte[] data)
        {
            Data = data;
            using PacketHandler packetHandler = new(data);
            int sharedSecretLength = packetHandler.ReadVarInt();
            SharedSecret = packetHandler.ReadUInt8Array(sharedSecretLength);
            int verifyTokenLength = packetHandler.ReadVarInt();
            VerifyToken = packetHandler.ReadUInt8Array(verifyTokenLength);

            SharedSecret = EncryptionRequestPacket.RSAKeyPair.Decrypt(SharedSecret, false);
            VerifyToken = EncryptionRequestPacket.RSAKeyPair.Decrypt(VerifyToken, false);
        }

        public EncryptionResponsePacket(byte[] sharedSecret, byte[] verifyToken)
        {
            SharedSecret = sharedSecret;
            VerifyToken = verifyToken;

            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculateLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteVarInt(sharedSecret.Length);
            packetHandler.WriteUnsignedBytes(sharedSecret);
            packetHandler.WriteVarInt(verifyToken.Length);
            packetHandler.WriteUnsignedBytes(verifyToken);
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadNextPacket().Data;
        }

        public override int CalculateLength() => Id.GetVarIntLength() + SharedSecret.Length.GetVarIntLength() + SharedSecret.Length + VerifyToken.Length.GetVarIntLength() + VerifyToken.Length;
    }
}