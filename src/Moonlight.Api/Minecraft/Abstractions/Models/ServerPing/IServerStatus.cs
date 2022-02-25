using Moonlight.Api.Minecraft.Objects.Chat;
using System.Text.Json.Serialization;

namespace Moonlight.Api.Minecraft.Abstractions.Models.ServerPing
{
    public interface IServerStatus
    {
        [JsonPropertyName("description")]
        ChatComponent Description { get; }

        [JsonPropertyName("version")]
        IServerVersion Version { get; }

        [JsonPropertyName("players")]
        IServerPlayers Players { get; }

        [JsonPropertyName("favicon")]
        string? Favicon { get; }

        static abstract string? GetFavicon();
    }
}