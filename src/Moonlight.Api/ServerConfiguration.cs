using System;
using Microsoft.Extensions.Configuration;

namespace Moonlight.Api
{
    public sealed record ServerConfiguration
    {
        private readonly IConfiguration _configuration;
        public const int ProtocolVersion = 761;

        public string Host { get; init; }
        public ushort Port { get; init; }
        public string Motd => _configuration.GetValue("Motd", "&#6b73dbA Moonlight Server");
        public int MaxPlayers => _configuration.GetValue("MaxPlayers", 100);
        public string ServerIconFilePath => _configuration.GetValue("ServerIconFilePath", "res/server-icon.png");
        public TimeSpan KeepAliveInterval => TimeSpan.FromSeconds(_configuration.GetValue("KeepAliveInterval", 10));
        public TimeSpan ClientTimeout => TimeSpan.FromSeconds(_configuration.GetValue("ClientTimeout", 30));

        public ServerConfiguration(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Host = _configuration.GetValue("Host", "127.0.0.1");
            Port = _configuration.GetValue<ushort>("Port", 25565);
        }
    }
}
