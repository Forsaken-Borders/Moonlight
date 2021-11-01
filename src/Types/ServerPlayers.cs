using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace Moonlight.Types
{
    public class ServerPlayers
    {
        [JsonPropertyName("max")]
        public int MaxPlayerCount { get; set; }

        [JsonPropertyName("online")]
        public int OnlinePlayerCount { get; set; }

        [JsonPropertyName("players")]
        public List<SamplePlayer> SamplePlayers { get; init; } = new();

        public ServerPlayers()
        {
            MaxPlayerCount = Program.Configuration.GetValue("server:max_players", 100);
            OnlinePlayerCount = 0;
            SamplePlayers = new();
        }

        public ServerPlayers(int maxPlayerCount, int onlinePlayerCount, List<SamplePlayer> samplePlayers)
        {
            MaxPlayerCount = maxPlayerCount;
            OnlinePlayerCount = onlinePlayerCount;
            SamplePlayers = samplePlayers;
        }

        public override bool Equals(object obj) => obj is ServerPlayers players && MaxPlayerCount == players.MaxPlayerCount && OnlinePlayerCount == players.OnlinePlayerCount && EqualityComparer<List<SamplePlayer>>.Default.Equals(SamplePlayers, players.SamplePlayers);
        public override int GetHashCode() => HashCode.Combine(MaxPlayerCount, OnlinePlayerCount, SamplePlayers);
    }
}
