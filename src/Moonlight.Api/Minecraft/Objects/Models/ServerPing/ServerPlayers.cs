using Moonlight.Api.Minecraft.Abstractions.Models.ServerPing;
using System.Text.Json.Serialization;

namespace Moonlight.Api.Minecraft.Objects.Models.ServerPing
{
    public class ServerPlayers : IServerPlayers
    {
        [JsonPropertyName("max")]
        public int MaxPlayerCount { get; set; }

        [JsonPropertyName("online")]
        public int OnlinePlayerCount { get; set; }

        [JsonPropertyName("players")]
        public IList<ISamplePlayer> SamplePlayers { get; init; } = new List<ISamplePlayer>();

        public ServerPlayers(int maxPlayerCount, int onlinePlayerCount, List<ISamplePlayer> samplePlayers)
        {
            ArgumentNullException.ThrowIfNull(maxPlayerCount, nameof(maxPlayerCount));
            ArgumentNullException.ThrowIfNull(onlinePlayerCount, nameof(onlinePlayerCount));
            ArgumentNullException.ThrowIfNull(samplePlayers, nameof(samplePlayers));

            MaxPlayerCount = maxPlayerCount;
            OnlinePlayerCount = onlinePlayerCount;
            SamplePlayers = samplePlayers;
        }

        public override int GetHashCode() => HashCode.Combine(MaxPlayerCount, OnlinePlayerCount, SamplePlayers);
        public override bool Equals(object? obj) => obj is ServerPlayers players && MaxPlayerCount == players.MaxPlayerCount && OnlinePlayerCount == players.OnlinePlayerCount && EqualityComparer<IList<ISamplePlayer>>.Default.Equals(SamplePlayers, players.SamplePlayers);
    }
}