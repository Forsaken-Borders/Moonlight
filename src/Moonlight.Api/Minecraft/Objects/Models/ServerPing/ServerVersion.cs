using Moonlight.Api.Minecraft.Abstractions.Models.ServerPing;
using System.Text.Json.Serialization;

namespace Moonlight.Api.Minecraft.Objects.Models.ServerPing
{
    public class ServerVersion : IServerVersion
    {
        public const string CurrentName = "1.17.1";
        public const int CurrentProtocol = 756;

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("protocol")]
        public int Protocol { get; set; }

        public ServerVersion(string name, int protocol)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            ArgumentNullException.ThrowIfNull(protocol, nameof(protocol));

            Name = name;
            Protocol = protocol;
        }
    }
}