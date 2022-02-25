using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Moonlight.Api.Server.JsonConverters
{
    internal class ColorJsonConverter : JsonConverter<Color>
    {
        public ColorJsonConverter() { }
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Color);
        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options) => writer.WriteStringValue(value == Color.Transparent ? "reset" : ColorTranslator.ToHtml(value));
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();
            return (value is "reset" or null) ? Color.Transparent : ColorTranslator.FromHtml(value);
        }
    }
}