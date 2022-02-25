using Moonlight.Api.Minecraft.Objects.Chat;
using System.Drawing;

namespace Moonlight.Api.Minecraft.Abstractions.Chat
{
    /// <summary>
    /// Represents a Minecraft Chat Component, used for sending text to players.
    /// </summary>
    public interface IChatComponent
    {
        /// <summary>
        /// The text of this component.
        /// </summary>
        string? Text { get; set; }

        /// <summary>
        /// Determines if <see cref="Text"/> is bold.
        /// </summary>
        bool? Bold { get; set; }

        /// <summary>
        /// Determines if <see cref="Text"/> is italic.
        /// </summary>
        bool? Italic { get; set; }

        /// <summary>
        /// Determines if <see cref="Text"/> is underlined.
        /// </summary>
        bool? Underlined { get; set; }

        /// <summary>
        /// Determines if <see cref="Text"/> is strikethrough.
        /// </summary>
        bool? Strikethrough { get; set; }

        /// <summary>
        /// Determines if <see cref="Text"/> is obfuscated.
        /// </summary>
        bool? Obfuscated { get; set; }

        /// <summary>
        /// Determines if <see cref="Text"/> holds a link.
        /// </summary>
        string? Insertation { get; set; }

        /// <summary>
        /// Determines if <see cref="Text"/> is colored.
        /// </summary>
        Color Color { get; set; }

        /// <summary>
        /// Determines which font <see cref="Text"/> uses.
        /// </summary>
        string? Font { get; set; }

        /// <summary>
        /// Holds the rest of the styled text.
        /// </summary>
        IChatComponent[]? Extra { get; set; }

        /// <summary>
        /// Tests if the component has any styled text.
        /// </summary>
        /// <returns>True if the <see cref="IChatComponent"/> holds any styled text, false otherwise.</returns>
        bool HasFormatting();

        /// <summary>
        /// Replaces the section signs (§) in <see cref="Text"/> and uses the proper properties to format the text.
        /// </summary>
        void Parse(string text);

        /// <summary>
        /// Creates a new <see cref="IChatComponent"/> from the string provided.
        /// </summary>
        /// <param name="text">The string to be parsed.</param>
        /// <returns>A new instance of <see cref="IChatComponent"/>.</returns>
        static abstract IChatComponent Create(string text);

        /// <summary>
        /// Converts the <see cref="IChatComponent"/> to a string.
        /// </summary>
        /// <param name="colorParseMode">Determines how the string is formatted, and if section signs should be used.</param>
        /// <param name="includeExtraComponents">Determines if the <see cref="Extra"/> array is appended to the string. If false, we convert just <see cref="Text"/>.</param>
        /// <returns>A formatted string using Section Signs (§).</returns>
        string ToString(ColorParseMode colorParseMode = ColorParseMode.Translate, bool includeExtraComponents = true);
    }
}