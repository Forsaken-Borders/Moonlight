using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace Moonlight.Api.Types.ServerPing
{
    public class ServerVersion
    {
        public const string CurrentName = "1.17.1";
        public const int CurrentProtocol = 756;

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("protocol")]
        public int Protocol { get; set; }

        public ServerVersion()
        {
            Name = Server.Config.GetValue("server:name", "Moonlight 1.17.1");
            Protocol = CurrentProtocol;
        }

        public ServerVersion(string name, int protocol)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            ArgumentNullException.ThrowIfNull(protocol, nameof(protocol));

            Name = name;
            Protocol = protocol;
        }
    }
}