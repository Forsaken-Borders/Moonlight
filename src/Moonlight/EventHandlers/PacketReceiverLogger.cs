using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moonlight.Api.Events.EventArgs;

namespace Moonlight.EventHandlers
{
    public sealed class PacketReceiverLogger
    {
        private readonly ILogger<PacketReceiverLogger> _logger;

        public PacketReceiverLogger(ILogger<PacketReceiverLogger> logger) => _logger = logger;

        public ValueTask<bool> LogPacketReceivedAsync(PacketReceivedAsyncServerEventArgs eventArgs)
        {
            _logger.LogInformation("Packet received: {Packet}", eventArgs.Packet);
            return ValueTask.FromResult(true);
        }
    }
}
