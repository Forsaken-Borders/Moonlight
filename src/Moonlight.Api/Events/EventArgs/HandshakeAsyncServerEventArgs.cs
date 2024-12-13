using Moonlight.Api.Net;
using Moonlight.Protocol.Net;

namespace Moonlight.Api.Events.EventArgs
{
    public sealed class HandshakeAsyncServerEventArgs : AsyncServerEventArgs
    {
        public required HandshakePacket HandshakePacket { get; init; }
        public required PacketReader PacketReader { get; init; }
    }
}
