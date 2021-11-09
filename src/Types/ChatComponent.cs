using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Moonlight.Types
{
    public class ChatComponent
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("bold")]
        public bool Bold { get; set; }

        [JsonPropertyName("italic")]
        public bool Italic { get; set; }

        [JsonPropertyName("underlined")]
        public bool Underlined { get; set; }

        [JsonPropertyName("strikethrough")]
        public bool Strikethrough { get; set; }

        [JsonPropertyName("obfuscated")]
        public bool Obfuscated { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("insertation")]
        public string Insertation { get; set; }

        [JsonPropertyName("extra")]
        public ChatComponent[] Extra { get; set; }

        public ChatComponent(string text) => Text = text;

        public static implicit operator ChatComponent(string text) => new(text);

        public override bool Equals(object obj) => obj is ChatComponent component && Text == component.Text && Bold == component.Bold && Italic == component.Italic && Underlined == component.Underlined && Strikethrough == component.Strikethrough && Obfuscated == component.Obfuscated && Color == component.Color && Insertation == component.Insertation && EqualityComparer<ChatComponent[]>.Default.Equals(Extra, component.Extra);

        public override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(Text);
            hash.Add(Bold);
            hash.Add(Italic);
            hash.Add(Underlined);
            hash.Add(Strikethrough);
            hash.Add(Obfuscated);
            hash.Add(Color);
            hash.Add(Insertation);
            hash.Add(Extra);
            return hash.ToHashCode();
        }
    }
}