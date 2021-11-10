using System;
using System.IO;
using System.Linq;

namespace Moonlight.Network.Packets
{
    public class EncryptionRequestPacket : Packet
    {
        public override int Id => 0x01;
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

        public override int CalculateLength() => Id.GetVarIntLength() + ServerId.Length.GetVarIntLength() + ServerId.Length + PublicKey.Length.GetVarIntLength() + PublicKey.Length + VerifyToken.Length.GetVarIntLength() + VerifyToken.Length;
    }
}