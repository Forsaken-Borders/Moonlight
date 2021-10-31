using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace Moonlight.Types
{
    public class MOTD
    {
        [JsonPropertyName("description")]
        public ChatComponent Description { get; set; } = new(Program.Configuration.GetValue("server:description", "The default description. The server probably isn't ready to be joined yet"));

        [JsonPropertyName("version")]
        public dynamic Version { get; set; } = new
        {
            Name = "1.17.1",
            Protocol = 756
        };

        [JsonPropertyName("players")]
        public dynamic Players { get; set; } = new
        {
            Max = 20,
            Online = 1,
            Sample = new
            {
                Name = "OoLunar",
                Id = "ee6d1622-f764-44d2-bb31-f5331f194166"
            },
        };
    }
}