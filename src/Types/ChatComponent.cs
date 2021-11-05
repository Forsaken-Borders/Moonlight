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
    }
}