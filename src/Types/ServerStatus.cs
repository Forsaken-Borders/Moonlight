using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace Moonlight.Types
{
    public class ServerStatus
    {
        [JsonPropertyName("description")]
        public ChatComponent Description { get; set; }

        [JsonPropertyName("version")]
        public ServerVersion Version { get; set; }

        [JsonPropertyName("players")]
        public ServerPlayers Players { get; set; }

        public ServerStatus()
        {
            Description = new(Program.Configuration.GetValue("server:description", "Moonlight: a C# implementation of the Minecraft Server Protocol."));
            Version = new();
            Players = new();
        }

        public ServerStatus(ChatComponent description, ServerVersion version, ServerPlayers players)
        {
            Description = description;
            Version = version;
            Players = players;
        }

        public override bool Equals(object obj) => obj is ServerStatus status && EqualityComparer<ChatComponent>.Default.Equals(Description, status.Description) && EqualityComparer<ServerVersion>.Default.Equals(Version, status.Version) && EqualityComparer<ServerPlayers>.Default.Equals(Players, status.Players);
        public override int GetHashCode() => HashCode.Combine(Description, Version, Players);
    }
}