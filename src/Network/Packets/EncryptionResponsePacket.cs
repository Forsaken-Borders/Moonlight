using System;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;

namespace Moonlight.Network.Packets
{
    public class EncryptionResponsePacket : Packet
    {
        public override int Id { get; init; } = 0x01;
        public byte[] SharedSecret { get; init; }
        public byte[] VerifyToken { get; init; }

        public EncryptionResponsePacket(byte[] data, AsymmetricKeyParameter privateKey)
        {
            ArgumentNullException.ThrowIfNull(data, nameof(data));
            ArgumentNullException.ThrowIfNull(privateKey, nameof(privateKey));
            Data = data;
            using PacketHandler packetHandler = new(data);
            int sharedSecretLength = packetHandler.ReadVarInt();
            SharedSecret = packetHandler.ReadUInt8Array(sharedSecretLength);
            int verifyTokenLength = packetHandler.ReadVarInt();
            VerifyToken = packetHandler.ReadUInt8Array(verifyTokenLength);

            SharedSecret = privateKey.ProcessEncryption(SharedSecret, false);
            VerifyToken = privateKey.ProcessEncryption(VerifyToken, false);
        }

        public EncryptionResponsePacket(byte[] sharedSecret, byte[] verifyToken, AsymmetricKeyParameter publicKey)
        {
            ArgumentNullException.ThrowIfNull(sharedSecret, nameof(sharedSecret));
            ArgumentNullException.ThrowIfNull(verifyToken, nameof(verifyToken));
            ArgumentNullException.ThrowIfNull(publicKey, nameof(publicKey));
            SharedSecret = sharedSecret;
            VerifyToken = verifyToken;

            IAsymmetricBlockCipher encryptCipher = new Pkcs1Encoding(new RsaEngine());
            encryptCipher.Init(true, publicKey);

            byte[] encryptedSharedSecret = encryptCipher.ProcessBlock(SharedSecret, 0, encryptCipher.GetInputBlockSize());
            byte[] encryptedVerifyToken = encryptCipher.ProcessBlock(VerifyToken, 0, encryptCipher.GetInputBlockSize());

            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculateLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteVarInt(encryptedSharedSecret.Length);
            packetHandler.WriteUnsignedBytes(encryptedSharedSecret);
            packetHandler.WriteVarInt(encryptedVerifyToken.Length);
            packetHandler.WriteUnsignedBytes(encryptedVerifyToken);
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadNextPacket().Data;
        }

        public override int CalculateLength() => Id.GetVarIntLength() + SharedSecret.Length.GetVarIntLength() + SharedSecret.Length + VerifyToken.Length.GetVarIntLength() + VerifyToken.Length;
    }
}