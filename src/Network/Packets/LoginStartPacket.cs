using System;

namespace Moonlight.Network.Packets
{
    public class LoginStartPacket : Packet
    {
        public string Username { get; init; }

        public LoginStartPacket(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data, nameof(data));
            Data = data;
            using PacketHandler packetHandler = new(data);
            Username = packetHandler.ReadString();
        }

        public override int CalculateLength() => Id.GetVarIntLength() + Username.Length.GetVarIntLength() + Username.Length;
    }
}