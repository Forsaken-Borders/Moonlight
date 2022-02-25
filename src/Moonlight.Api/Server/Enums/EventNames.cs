namespace Moonlight.Api.Server.Enums
{
    /// <summary>
    /// A list of events to subscribe too.
    /// </summary>
    public enum EventName
    {
        /// <summary>
        /// Called when all plugins have been loaded.
        /// </summary>
        ServerStarted,

        /// <summary>
        /// Called when the server is shutting down.
        /// </summary>
        ServerStopping,

        /// <summary>
        /// Called when a Minecraft packet is being received.
        /// </summary>
        PacketReceiving,

        /// <summary>
        /// Called when a Minecraft packet has been received.
        /// </summary>
        PacketReceived,

        /// <summary>
        /// Called when a Minecraft packet is being sent.
        /// </summary>
        PacketSent,

        /// <summary>
        /// Called when the server has been pinged for it's MOTD.
        /// </summary>
        ServerListPing
    }
}