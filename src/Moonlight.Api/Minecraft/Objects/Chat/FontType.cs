namespace Moonlight.Api.Minecraft.Objects.Chat
{
    /// <summary>
    /// Switches between the default Minecraft font, the unicode font and the enchanting table font. Does NOT override texture packs.
    /// </summary>
    public enum FontType
    {
        /// <summary>
        /// Uses the default Minecraft font.
        /// </summary>
        Default,

        /// <summary>
        /// Uses the unicode font.
        /// </summary>
        Uniform,

        /// <summary>
        /// Uses the enchanting table font.
        /// </summary>
        Alt
    };
}