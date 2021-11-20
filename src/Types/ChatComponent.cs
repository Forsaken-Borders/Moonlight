using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Text.Json.Serialization;

namespace Moonlight.Types
{
    public enum ColorParseMode
    {
        None = 0,
        Legacy = 1,
        Translate = 2,
        Hex = 3
    };

    public enum FontType
    {
        Default,
        Uniform,
        Alt
    };

    public class ChatComponent
    {
        public string Text { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underlined { get; set; }
        public bool Strikethrough { get; set; }
        public bool Obfuscated { get; set; }
        public string Insertation { get; set; }
        public ChatComponent[] Extra { get; set; }
        public string Font { get; set; }

        [JsonIgnore]
        public Color Color { get; set; }
        [JsonPropertyName("color"), SuppressMessage("Roslyn", "IDE0025", Justification = "Intentionally shadowing the property behind Color, but HexColor needs to be a public field due to STJ things.")]
        public string HexColor { get => Color == Color.Transparent ? "reset" : ColorTranslator.ToHtml(Color); }

        public ChatComponent(string text, bool parse = true)
        {
            if (parse)
            {
                Parse(text);
            }
            else
            {
                Text = text;
            }
        }

        // TODO: Check if there's any legacy formatting in the Text (Text.Contains('§') is not enough since § can be by itself without any formatting occuring).
        public bool HasFormatting() => Bold || Italic || Underlined || Strikethrough || Obfuscated || string.IsNullOrEmpty(Insertation?.Trim()) || Extra.Length != 0 || string.IsNullOrEmpty(Font?.Trim());
        public void SetFont(FontType type) => Font = "minecraft:" + Enum.GetName(type).ToLowerInvariant();
        public static implicit operator ChatComponent(string text) => new(text);
        public override string ToString() => ToString(ColorParseMode.None, true);
        public override bool Equals(object obj) => obj is ChatComponent component && Text == component.Text && Bold == component.Bold && Italic == component.Italic && Underlined == component.Underlined && Strikethrough == component.Strikethrough && Obfuscated == component.Obfuscated && Insertation == component.Insertation && EqualityComparer<ChatComponent[]>.Default.Equals(Extra, component.Extra) && Font == component.Font && Color.Equals(component.Color) && HexColor == component.HexColor;
        public override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(Text);
            hash.Add(Bold);
            hash.Add(Italic);
            hash.Add(Underlined);
            hash.Add(Strikethrough);
            hash.Add(Obfuscated);
            hash.Add(Insertation);
            hash.Add(Extra);
            hash.Add(Font);
            hash.Add(Color);
            hash.Add(HexColor);
            return hash.ToHashCode();
        }

        public void Parse(string text)
        {
            StringBuilder builder = new();
            List<ChatComponent> extra = new();
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] is '§' or '&')
                {
                    if ((i + 1) >= text.Length)
                    {
                        builder.Append(text[i]);
                        continue;
                    }

                    if (builder.Length != 0)
                    {
                        extra.Add(new ChatComponent(text[i..]));
                        i = text.Length;
                        break;
                    }

                    switch (text[i + 1])
                    {
                        case '&':
                            builder.Append('&');
                            break;
                        case '§':
                            builder.Append('§');
                            break;
                        case '@':
                            if (i + 2 >= text.Length)
                            {
                                builder.Append('@');
                                break;
                            }

                            switch (text[i + 2])
                            {
                                case '1':
                                    SetFont(FontType.Default);
                                    break;
                                case '2':
                                    SetFont(FontType.Uniform);
                                    break;
                                case '3':
                                    SetFont(FontType.Alt);
                                    break;
                                default:
                                    builder.Append('@');
                                    builder.Append(text[i + 2]);
                                    break;
                            }
                            i++;
                            break;
                        case 'k':
                            Obfuscated = true;
                            break;
                        case 'l':
                            Bold = true;
                            break;
                        case 'm':
                            Strikethrough = true;
                            break;
                        case 'n':
                            Underlined = true;
                            break;
                        case 'o':
                            Italic = true;
                            break;
                        case 'r':
                            Color = Color.Transparent;
                            break;
                        case '#':
                            if ((i + 7) > text.Length)
                            {
                                goto default;
                            }
                            Color hexColor;
                            if (ChatColors.IsValidHex(text.Substring(i + 1, 7)))
                            {
                                hexColor = ColorTranslator.FromHtml(text.Substring(i + 1, 7));
                                Color = hexColor;
                                i += 6;
                                break;
                            }
                            else
                            {
                                goto default;
                            }
                        default:
                            hexColor = ChatColors.GetColor(text[i + 1]);
                            if (hexColor != Color.Empty)
                            {
                                Color = hexColor;
                            }
                            else
                            {
                                builder.Append(text[i]);
                                builder.Append(text[i + 1]);
                            }
                            break;
                    }
                    i++;
                }
                else
                {
                    builder.Append(text[i]);
                }
            }

            Text = builder.ToString();
            Extra = extra.Count != 0 ? extra.ToArray() : null;
        }

        public string ToString(ColorParseMode colorParseMode = ColorParseMode.Translate, bool includeExtraComponents = true)
        {
            StringBuilder stringBuilder = new();
            switch (colorParseMode)
            {
                case ColorParseMode.None:
                    for (int i = 0; i < Text.Length; i++)
                    {
                        if (Text[i] == '§')
                        {
                            char c = Text[i + 1];
                            // If c is between 0-f, or between k-r, skip it and continue.
                            if (c is (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'k' and <= 'r'))
                            {
                                i++;
                                continue;
                            }
                        }
                        stringBuilder.Append(Text[i]);
                    }

                    if (includeExtraComponents)
                    {
                        if (Extra != null)
                        {
                            foreach (ChatComponent chatComponent in Extra)
                            {
                                stringBuilder.Append(chatComponent.ToString(colorParseMode, includeExtraComponents));
                            }
                        }
                    }
                    return stringBuilder.ToString();
                case ColorParseMode.Legacy:
                    stringBuilder.Append(ConvertPropertiesToSectionSigns());
                    stringBuilder.Append(Text);
                    if (includeExtraComponents)
                    {
                        if (Extra != null)
                        {
                            foreach (ChatComponent chatComponent in Extra)
                            {
                                stringBuilder.Append(chatComponent.ToString(colorParseMode, includeExtraComponents));
                            }
                        }
                    }
                    return stringBuilder.ToString();
                case ColorParseMode.Translate:
                    stringBuilder.Append('§');
                    stringBuilder.Append(ChatColors.GetColorCode(ChatColors.GetClosestColor(ChatColors.GetColors(), Color)));
                    goto case ColorParseMode.Legacy;
                case ColorParseMode.Hex:
                    throw new ArgumentException("Hex color codes cannot be translated to legacy formatting strings!");
                default:
                    throw new ArgumentOutOfRangeException(nameof(colorParseMode), colorParseMode, $"Unsure how to handle {Enum.GetName(colorParseMode)} as it hasn't been implemented. Free PR available!");
            }
        }

        private string ConvertPropertiesToSectionSigns()
        {
            StringBuilder stringBuilder = new();

            if (Obfuscated)
            {
                stringBuilder.Append("§k");
            }

            if (Bold)
            {
                stringBuilder.Append("§l");
            }

            if (Strikethrough)
            {
                stringBuilder.Append("§m");
            }

            if (Underlined)
            {
                stringBuilder.Append("§n");
            }

            if (Italic)
            {
                stringBuilder.Append("§o");
            }

            return stringBuilder.ToString();
        }
    }
}