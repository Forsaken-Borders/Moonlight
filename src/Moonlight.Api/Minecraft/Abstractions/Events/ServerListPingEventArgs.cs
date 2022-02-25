using Moonlight.Api.Minecraft.Objects.Networking;
using Moonlight.Api.Minecraft.Objects.Networking.Packets.StatusState;

namespace Moonlight.Api.Minecraft.Abstractions.Events
{
    public class ServerListPingEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
        public ResponsePacket ResponsePacket { get; set; }
        public PacketHandler PacketHandler { get; }

        public ServerListPingEventArgs(PacketHandler packetHandler, ResponsePacket responsePacket)
        {
            PacketHandler = packetHandler;
            ResponsePacket = responsePacket;
        }
    }
}