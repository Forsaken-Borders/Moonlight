using System.Collections.Generic;
using System.Text.Json.Serialization;
using Moonlight.Protocol.Components.Chat;

namespace Moonlight.Protocol.Components
{
    public record Component
    {
        [JsonPropertyName("bold")]
        public bool Bold { get; init; }

        [JsonPropertyName("italic")]
        public bool Italic { get; init; }

        [JsonPropertyName("underlined")]
        public bool Underlined { get; init; }

        [JsonPropertyName("strikethrough")]
        public bool Strikethrough { get; init; }

        [JsonPropertyName("obfuscated")]
        public bool Obfuscated { get; init; }

        [JsonPropertyName("font")]
        public string? Font { get; init; }

        [JsonPropertyName("color")]
        public string? Color { get; init; }

        [JsonPropertyName("insertion")]
        public string? Insertion { get; init; }

        [JsonPropertyName("clickEvent")]
        public ChatClickEventComponent? ClickEvent { get; init; }

        [JsonPropertyName("hoverEvent")]
        public ChatHoverEventComponent? HoverEvent { get; init; }

        [JsonPropertyName("extra")]
        public IReadOnlyList<Component> Extra { get; init; } = new List<Component>();
    }
}
