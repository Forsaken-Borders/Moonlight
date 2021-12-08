using Microsoft.Extensions.Configuration;
using Moonlight.Api.Networking;
using Serilog;

namespace Moonlight.Api
{
    public static class Server
    {
        private static bool Initiated;
        public static IConfiguration Config { get; private set; } = null!;
        public static ILogger Logger { get; private set; } = null!;

        public static void Init(IConfiguration config, ILogger logger, bool force = false)
        {
            if (Initiated && !force)
            {
                throw new InvalidOperationException("Server has already been initiated, and the force argument was set to false.");
            }

            Initiated = true;
            Config = config;
            Logger = logger;
            MinecraftAPI.HttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Moonlight/2.0.0 (1.17.1)");
        }
    }
}