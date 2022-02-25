using System.Text.Json.Serialization;

namespace Moonlight.Api.Minecraft.Abstractions.Models.ServerPing
{
    public interface IServerPlayers
    {
        [JsonPropertyName("max")]
        int MaxPlayerCount { get; }

        [JsonPropertyName("online")]
        int OnlinePlayerCount { get; }

        [JsonPropertyName("players")]
        IList<ISamplePlayer> SamplePlayers { get; }
    }
}
