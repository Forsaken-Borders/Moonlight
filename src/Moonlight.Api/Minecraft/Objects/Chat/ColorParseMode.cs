namespace Moonlight.Api.Minecraft.Objects.Chat
{
    /// <summary>
    /// How to parse the string to a <see cref="ChatComponent"/>.
    /// </summary>
    public enum ColorParseMode
    {
        /// <summary>
        /// Don't parse the string at all, leave it as is.
        /// </summary>
        None = 0,

        /// <summary>
        /// Use the § symbols to convert to color codes.
        /// </summary>
        Legacy = 1,

        /// <summary>
        /// Translates hex color codes to Minecraft color codes.
        /// </summary>
        Translate = 2,

        /// <summary>
        /// Uses the latest hex color codes. Only supported from 1.16+
        /// </summary>
        Hex = 3
    };
}