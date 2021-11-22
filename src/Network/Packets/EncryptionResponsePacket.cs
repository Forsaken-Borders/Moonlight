using System;
using Org.BouncyCastle.Crypto;

namespace Moonlight.Network.Packets;

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

    public override int CalculateLength() => Id.GetVarIntLength() + SharedSecret.Length.GetVarIntLength() + SharedSecret.Length + VerifyToken.Length.GetVarIntLength() + VerifyToken.Length;
}
