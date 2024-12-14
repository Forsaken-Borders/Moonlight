using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Moonlight.Protocol.Components.Chat;

namespace Moonlight.Protocol.Components
{
    public record Component
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Bold { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Italic { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Underlined { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Strikethrough { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Obfuscated { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Font { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Color { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Insertion { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyList<Component>? Extra { get; init; } = Array.Empty<Component>();

        [JsonPropertyName("clickEvent"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ChatClickEventComponent? ClickEvent { get; init; }

        [JsonPropertyName("hoverEvent"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ChatHoverEventComponent? HoverEvent { get; init; }

        public Component() { }
    }
}
