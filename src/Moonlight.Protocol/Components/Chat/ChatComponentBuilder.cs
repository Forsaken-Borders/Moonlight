using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Moonlight.Protocol.Components.Chat
{
    public sealed class ChatComponentBuilder
    {
        public required string Text { get; set; }
        public ChatFormatting Formatting { get; set; } = ChatFormatting.None;
        public string? Insertion { get; set; }
        public string? Font { get; set; } = "minecraft:default";
        public Color? Color { get; set; }
        public ChatClickEventComponent? ClickEvent { get; set; }
        public ChatHoverEventComponent? HoverEvent { get; set; }
        public List<ChatComponentBuilder> Extra { get; set; } = [];
        public string? HexColor => Color is null ? null : ColorTranslator.ToHtml(Color.Value);

        public ChatComponentBuilder() { }

        public void SetFont(ChatFontType font) => Font = "minecraft:" + font.ToString().ToLowerInvariant();
        public void SetFont<TEnum>(TEnum font, string @namespace = "minecraft") where TEnum : struct, Enum => Font = $"{@namespace}:{Enum.GetName(font)}";
        public void SetColor(Color color) => Color = color;
        public void SetColor(string hexColor) => Color = ColorTranslator.FromHtml(hexColor);
        public bool IsPlainText() => Formatting == ChatFormatting.None && string.IsNullOrEmpty(Insertion) && Extra.All(x => x.IsPlainText()) && Font == "minecraft:default" && Color is null;

        public static ChatComponentBuilder Parse(string text)
        {
            ChatComponentBuilder builder = new()
            {
                Text = string.Empty
            };

            ChatComponentBuilder current = builder;
            StringBuilder textBuffer = new();
            ReadOnlySpan<char> span = text.AsSpan();
            for (int i = 0; i < span.Length; i++)
            {
                // Check if the current character is a formatting character and ensure that there is a possible formatting code after it.
                if (span[i] is not FormattingCodes.SectionSign and not FormattingCodes.Ampersand)
                {
                    textBuffer.Append(span[i]);
                    continue;
                }
                // Attempt to skip over the formatting character but if we're at the end of the string, just break.
                else if (++i >= span.Length)
                {
                    textBuffer.Append(span[i - 1]);
                    break;
                }
                // Check to see if we've found any text yet. If we have, we need to create a new component for it.
                else if (textBuffer.Length > 0)
                {
                    current.Text = textBuffer.ToString();
                    textBuffer.Clear();
                    current = new()
                    {
                        Text = string.Empty
                    };

                    builder.Extra.Add(current);
                }

                // Check if the next character is a valid formatting code.
                if (FormattingCodes.StyleCodes.TryGetValue(span[i], out ChatFormatting formatting))
                {
                    builder.Formatting |= formatting;
                }
                // Check if the next character is a valid color code.
                else if (span[i] == '#' && FormattingCodes.ColorCodes.TryGetValue(span[i], out Color color))
                {
                    builder.Color = color;
                }
                // Check if the next character is a font code.
                else if (span[i] == '$')
                {
                    switch (span[i + 1])
                    {
                        case '1':
                            builder.SetFont(ChatFontType.Default);
                            break;
                        case '2':
                            builder.SetFont(ChatFontType.Uniform);
                            break;
                        case '3':
                            builder.SetFont(ChatFontType.Alt);
                            break;
                        default:
                            i--;
                            break;
                    }
                    i++;
                }
                else
                {
                    textBuffer.Append(span[i - 1]);
                    textBuffer.Append(span[i]);
                }
            }

            // Check if we have any text left over and if so, add it to the current component.
            if (textBuffer.Length > 0)
            {
                current.Text = textBuffer.ToString();
            }

            return builder;
        }

        public string ToString(ChatColorMode colorMode, bool includeExtraComponents = true)
        {
            StringBuilder builder = new();
            for (int i = 0; i < sizeof(ChatFormatting) * 8; i++)
            {
                ChatFormatting flag = (ChatFormatting)(1 << i);
                if (Formatting.HasFlag(flag))
                {
                    builder.Append(FormattingCodes.SectionSign);
                    builder.Append(FormattingCodes.StyleCodes.First(x => x.Value == flag).Key);
                }
            }

            builder.Append(FormattingCodes.SectionSign);
            builder.Append(FormattingCodes.StyleCodes.First(x => x.Value == Formatting).Key);
            if (Color is not null)
            {
                switch (colorMode)
                {
                    case ChatColorMode.None:
                        break;
                    case ChatColorMode.Legacy:
                        char legacyColor = FormattingCodes.ColorCodes.FirstOrDefault(x => x.Value == Color).Key;
                        if (legacyColor != default)
                        {
                            builder.Append(FormattingCodes.SectionSign);
                            builder.Append(legacyColor);
                        }
                        break;
                    case ChatColorMode.Translate:
                        builder.Append(FormattingCodes.SectionSign);
                        builder.Append(FormattingCodes.GetClosestColor(FormattingCodes.ColorNames.Values, Color.Value));
                        break;
                    case ChatColorMode.Hex:
                        builder.Append(FormattingCodes.SectionSign);
                        builder.Append('#');
                        builder.Append(HexColor);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(colorMode), colorMode, "Invalid color mode.");
                }
            }

            builder.Append(Text);
            if (includeExtraComponents)
            {
                foreach (ChatComponentBuilder extra in Extra)
                {
                    builder.Append(extra.ToString(colorMode, includeExtraComponents));
                }
            }

            return builder.ToString();
        }
    }
}
