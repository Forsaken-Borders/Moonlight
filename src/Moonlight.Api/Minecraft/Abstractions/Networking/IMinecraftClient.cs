using Moonlight.Api.Minecraft.Objects.Chat;
using Org.BouncyCastle.Crypto;

namespace Moonlight.Api.Minecraft.Abstractions.Networking
{
    public interface IMinecraftClient : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// The packet handler used to talk between the client and the server.
        /// </summary>
        IPacketHandler PacketHandler { get; }

        /// <summary>
        /// The key pair used to encrypt and decrypt packets. Only null when encryption isn't being used.
        /// </summary>
        AsymmetricCipherKeyPair? KeyPair { get; }

        /// <summary>
        /// Whether the client is connected via localhost.
        /// </summary>
        bool IsLocalhost { get; }

        /// <summary>
        /// After the handshake is complete, this method executes the login process, which entails sending encryption packets, compression packets and eventually the login packets.
        /// </summary>
        /// <returns>A task entailing the final packet transmission.</returns>
        Task Login();


        /// <summary>
        /// Sends a disconnect packet to the client, optionally specifying a reason (Chat formatting supported in 1.13+).
        /// </summary>
        /// <param name="reason">Why was the client disconnected from the server?</param>
        /// <returns>A task entailing the final packet transmission.</returns>
        Task DisconnectAsync(ChatComponent? reason = null);
    }
}