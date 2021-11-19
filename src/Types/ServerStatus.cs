using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

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

        [JsonPropertyName("favicon")]
        public string Favicon { get; set; } = GetFavicon();

        public ServerStatus()
        {
            Description = new(Program.Configuration.GetValue("server:description", "Moonlight: a C# implementation of the Minecraft Server Protocol."));
            Version = new();
            Players = new();
        }

        public ServerStatus(ChatComponent description, ServerVersion version, ServerPlayers players)
        {
            ArgumentNullException.ThrowIfNull(description, nameof(description));
            ArgumentNullException.ThrowIfNull(version, nameof(version));
            ArgumentNullException.ThrowIfNull(players, nameof(players));

            Description = description;
            Version = version;
            Players = players;
        }

        public static string GetFavicon()
        {
            string serverIconPath = FileUtils.GetConfigPath() + "server_icon.png";
            if (!File.Exists(serverIconPath))
            {
                return null;
            }
            Image serverIcon = Image.Load(serverIconPath);
            serverIcon.Mutate(image => image.Resize(64, 64));
            return serverIcon.ToBase64String(PngFormat.Instance);
        }

        public override bool Equals(object obj) => obj is ServerStatus status && EqualityComparer<ChatComponent>.Default.Equals(Description, status.Description) && EqualityComparer<ServerVersion>.Default.Equals(Version, status.Version) && EqualityComparer<ServerPlayers>.Default.Equals(Players, status.Players) && Favicon == status.Favicon;
        public override int GetHashCode() => HashCode.Combine(Description, Version, Players, Favicon);
    }
}