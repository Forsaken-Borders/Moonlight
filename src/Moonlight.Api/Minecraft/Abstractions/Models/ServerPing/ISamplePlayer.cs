using System.Text.Json.Serialization;

namespace Moonlight.Api.Minecraft.Abstractions.Models.ServerPing
{
    public interface ISamplePlayer
    {
        [JsonPropertyName("id")]
        Guid Id { get; }

        [JsonPropertyName("name")]
        string? Name { get; }
    }
}