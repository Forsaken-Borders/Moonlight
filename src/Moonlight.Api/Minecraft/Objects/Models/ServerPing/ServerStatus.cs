using Moonlight.Api.Minecraft.Abstractions.Models.ServerPing;
using Moonlight.Api.Minecraft.Objects.Chat;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System.Text.Json.Serialization;

namespace Moonlight.Api.Minecraft.Objects.Models.ServerPing
{
    public class ServerStatus : IServerStatus
    {
        [JsonPropertyName("description")]
        public ChatComponent Description { get; set; }

        [JsonPropertyName("version")]
        public IServerVersion Version { get; set; }

        [JsonPropertyName("players")]
        public IServerPlayers Players { get; set; }

        [JsonPropertyName("favicon")]
        public string? Favicon { get; set; }

        public ServerStatus(ChatComponent description, ServerVersion version, ServerPlayers players)
        {
            ArgumentNullException.ThrowIfNull(description, nameof(description));
            ArgumentNullException.ThrowIfNull(version, nameof(version));
            ArgumentNullException.ThrowIfNull(players, nameof(players));

            Description = description;
            Version = version;
            Players = players;
        }

        public static string? GetFavicon()
        {
            string serverIconPath = "./server_icon.png";
            if (!File.Exists(serverIconPath))
            {
                return null;
            }
            Image serverIcon = Image.Load(serverIconPath);
            serverIcon.Mutate(image => image.Resize(64, 64));
            return serverIcon.ToBase64String(PngFormat.Instance);
        }

        public override int GetHashCode() => HashCode.Combine(Description, Version, Players, Favicon);
        public override bool Equals(object? obj) => obj is ServerStatus status && EqualityComparer<ChatComponent>.Default.Equals(Description, status.Description) && EqualityComparer<IServerVersion>.Default.Equals(Version, status.Version) && EqualityComparer<IServerPlayers>.Default.Equals(Players, status.Players) && Favicon == status.Favicon;
    }
}