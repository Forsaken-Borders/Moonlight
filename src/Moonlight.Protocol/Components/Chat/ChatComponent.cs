using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Moonlight.Protocol.Components.Chat
{
    public record ChatComponent : Component
    {
        [JsonPropertyName("text")]
        public string Text { get; init; }

        public ChatComponent(ChatComponentBuilder builder)
        {
            Text = builder.Text;
            Bold = builder.Formatting.HasFlag(ChatFormatting.Bold);
            Italic = builder.Formatting.HasFlag(ChatFormatting.Italic);
            Underlined = builder.Formatting.HasFlag(ChatFormatting.Underline);
            Strikethrough = builder.Formatting.HasFlag(ChatFormatting.Strikethrough);
            Obfuscated = builder.Formatting.HasFlag(ChatFormatting.Obfuscated);
            Color = builder.HexColor;
            ClickEvent = builder.ClickEvent;
            HoverEvent = builder.HoverEvent;
            Insertion = builder.Insertion;
            Extra = builder.Extra.Select(x => new ChatComponent(x)).ToArray();
        }

        public ChatComponent(params string[] text) : this(new ChatComponentBuilder(text[0]))
        {
            List<ChatComponent> extra = new();
            for (int i = 1; i < text.Length; i++)
            {
                extra.Add(new ChatComponent(ChatComponentBuilder.Parse(text[i])));
            }
        }
    }
}
