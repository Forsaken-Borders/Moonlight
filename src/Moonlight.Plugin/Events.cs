using Moonlight.Api.Minecraft.Abstractions.Events;
using Moonlight.Api.Server.Attributes;
using Moonlight.Api.Server.Enums;

namespace Moonlight.MoonshadePlugin
{
    public class Events
    {
        [SubscribeToEvent(EventName.ServerListPing, EventPriority.Normal)]
        public async Task ServerListPingAsync(ServerListPingEventArgs eventArgs)
        {
            eventArgs.Cancel = true;
            eventArgs.ResponsePacket.Payload.Description = "&2You're banned from this server.";
            await eventArgs.PacketHandler.WritePacketAsync(eventArgs.ResponsePacket);
        }
    }
}