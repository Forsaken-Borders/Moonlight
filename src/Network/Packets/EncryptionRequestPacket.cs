using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Moonlight.Network.Packets
{
    public class EncryptionRequestPacket : Packet
    {
        [SuppressMessage("Roslyn", "CA5385", Justification = "Minecraft protocol specifies 1024-bit key")]
        public static RSACryptoServiceProvider RSAKeyPair { get; } = new(1024);
        public static string StaticServerId { get; } = new(Enumerable.Range(0, 20).Select(n => (char)new Random().Next('A', 'Z' + 1)).ToArray());

        public string ServerId { get; init; } = StaticServerId;
        public byte[] PublicKey { get; init; }
        public byte[] VerifyToken { get; init; }

        public EncryptionRequestPacket(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data, nameof(data));
            Data = data;

            using PacketHandler packetHandler = new(data);
            ServerId = packetHandler.ReadString();
            PublicKey = packetHandler.ReadUInt8Array(packetHandler.ReadVarInt());
            VerifyToken = packetHandler.ReadUInt8Array(packetHandler.ReadVarInt());
        }

        public EncryptionRequestPacket(byte[] publicKey, byte[] verifyToken)
        {
            ArgumentNullException.ThrowIfNull(publicKey, nameof(publicKey));
            ArgumentNullException.ThrowIfNull(verifyToken, nameof(verifyToken));
            PublicKey = publicKey;
            VerifyToken = verifyToken;

            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculateLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteString(ServerId);
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
            PublicKey = RSAKeyPair.ExportCspBlob(false);
            VerifyToken = new byte[4];
            random.NextBytes(VerifyToken);

            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculateLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteString(ServerId);
            packetHandler.WriteVarInt(PublicKey.Length);
            packetHandler.WriteUnsignedBytes(PublicKey);
            packetHandler.WriteVarInt(VerifyToken.Length);
            packetHandler.WriteUnsignedBytes(VerifyToken);
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadNextPacket().Data;
        }

        public override int CalculateLength() => Id.GetVarIntLength() + ServerId.Length.GetVarIntLength() + ServerId.Length + PublicKey.Length.GetVarIntLength() + PublicKey.Length + VerifyToken.Length.GetVarIntLength() + VerifyToken.Length;
    }
}