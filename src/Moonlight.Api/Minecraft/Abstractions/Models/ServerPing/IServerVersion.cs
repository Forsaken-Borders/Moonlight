using System.Text.Json.Serialization;

namespace Moonlight.Api.Minecraft.Abstractions.Models.ServerPing
{
    public interface IServerVersion
    {
        [JsonPropertyName("name")]
        string Name { get; }

        [JsonPropertyName("protocol")]
        int Protocol { get; }
    }
}