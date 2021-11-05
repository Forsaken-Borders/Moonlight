using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;

namespace Moonlight.Network.Packets
{
    public class EncryptionRequestPacket : Packet
    {
        [SuppressMessage("Roslyn", "CA5385", Justification = "Minecraft protocol specifies 1024-bit key")]
        public static RSACryptoServiceProvider RSAKeyPair { get; } = new(1024);

        public byte[] PublicKey { get; init; }
        public byte[] VerifyToken { get; init; }

        public EncryptionRequestPacket(byte[] publicKey, byte[] verifyToken)
        {
            PublicKey = publicKey;
            VerifyToken = verifyToken;

            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculateLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteVarInt(PublicKey.Length);
            packetHandler.WriteUnsignedBytes(PublicKey);
            packetHandler.WriteVarInt(VerifyToken.Length);
            packetHandler.WriteUnsignedBytes(VerifyToken);
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadNextPacket().Data;
        }

        public EncryptionRequestPacket(Random random = null)
        {
            random ??= new();
            PublicKey = RSAKeyPair.ExportRSAPublicKey();
            VerifyToken = new byte[4];
            random.NextBytes(VerifyToken);

            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculateLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteVarInt(PublicKey.Length);
            packetHandler.WriteUnsignedBytes(PublicKey);
            packetHandler.WriteVarInt(VerifyToken.Length);
            packetHandler.WriteUnsignedBytes(VerifyToken);
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadNextPacket().Data;
        }

        public override int CalculateLength() => Id.GetVarIntLength() + PublicKey.Length.GetVarIntLength() + PublicKey.Length + VerifyToken.Length.GetVarIntLength() + VerifyToken.Length;
    }
}