using Moonlight.Api.Net;
using Moonlight.Protocol.Net;

namespace Moonlight.Api.Events.EventArgs
{
    public sealed class PacketReceivedAsyncServerEventArgs : AsyncServerEventArgs
    {
        public PacketReader Reader { get; init; }
        public IPacket Packet { get; init; }

        public PacketReceivedAsyncServerEventArgs(IPacket packet, PacketReader reader)
        {
            Packet = packet;
            Reader = reader;
        }
    }
}
