using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Moonlight.Protocol.Components.Chat
{
    public record ChatComponent : Component
    {
        public required string Text { get; init; }

        [SetsRequiredMembers]
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
            Extra = builder.Extra.Count == 0 ? null : builder.Extra.Select(x => new ChatComponent(x)).ToArray();
        }

        [SetsRequiredMembers]
        public ChatComponent(params string[] text) : this(ChatComponentBuilder.Parse(text[0]))
        {
            List<ChatComponent> extra = [];
            for (int i = 1; i < text.Length; i++)
            {
                extra.Add(new ChatComponent(ChatComponentBuilder.Parse(text[i])));
            }
        }
    }
}
