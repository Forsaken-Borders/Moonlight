using Moonlight.Api.Net;
using Moonlight.Protocol.Net.HandshakeState;

namespace Moonlight.Api.Events.EventArgs
{
    public sealed class HandshakeAsyncServerEventArgs : AsyncServerEventArgs
    {
        public required HandshakePacket HandshakePacket { get; init; }
        public required PacketHandler PacketReader { get; init; }
    }
}
