using System.Threading.Tasks;
using Moonlight.Api;
using Moonlight.Api.Events.EventArgs;
using Moonlight.Protocol.Components.Chat;
using Moonlight.Protocol.Net.StatusState;

namespace Moonlight.EventHandlers
{
    public sealed class HandshakeReceivedHandler
    {
        private readonly ServerConfiguration _configuration;

        public HandshakeReceivedHandler(ServerConfiguration configuration) => _configuration = configuration;

        public async ValueTask HandleHandshakeAsync(PacketReceivedAsyncServerEventArgs eventArgs)
        {
            if (eventArgs.Packet is not StatusRequestPacket)
            {
                return;
            }

            eventArgs.PacketHandler.WritePacket(new StatusResponsePacket()
            {
                Version = new StatusResponsePacket.ServerVersion()
                {
                    Name = "1.21.1",
                    Protocol = 767
                },
                Players = new StatusResponsePacket.ServerPlayers()
                {
                    Max = _configuration.MaxPlayers,
                    Online = 0
                },
                Description = new ChatComponent(_configuration.Motd),
                EnforcesSecureChat = true,
                Favicon = StatusResponsePacket.GetServerIcon(_configuration.ServerIconFilePath)
            });

            await eventArgs.PacketHandler.FlushAsync();
            eventArgs.PacketHandler.Dispose();
        }
    }
}
