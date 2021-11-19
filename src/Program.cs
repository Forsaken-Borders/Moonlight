using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moonlight.Logging;
using Moonlight.Network;
using Serilog;
using Serilog.Events;
using SixLabors.ImageSharp.Formats.Png;

namespace Moonlight
{
    public class Server
    {
        internal static IConfiguration Configuration { get; set; }
        internal static IServiceProvider ServiceProvider { get; set; }
        internal static Serilog.ILogger Logger { get; set; }

        public static async Task Main(string[] args)
        {
            /* Use the ASP.NET Core configuration library to load up either a yml or json file.
             * We are intentionally loading things in the following order:
             * - Yml config
             * - Json config
             * - Environment variables
             * - Command line arguments
             * Env vars and cmd args are intentionally loaded after the config files to allow either easy debugging
             * or to allow use for future server host providers, such as MCProHosting (Not affiliated).
             */
            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.Sources.Clear(); // Remove the default configuration sources.
            configurationBuilder.AddYamlFile(FileUtils.GetConfigPath() + "config.yml", true, true);
            configurationBuilder.AddJsonFile(FileUtils.GetConfigPath() + "config.json", true, true);
            configurationBuilder.AddEnvironmentVariables("MOONLIGHT_");
            configurationBuilder.AddCommandLine(args);
            Configuration = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(Configuration); // The future plugin system will have constructor DI support. The server configuration should be easily grabbable from there.
            services.AddLogging(loggingBuilder => // Setup logging with default values specified, in case no configs, env vars or cmd args are provided. Default values still allow for clean error codes, which are somewhat helpful in debugging.
            {
                LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                    .Enrich.WithThreadId()
                    .MinimumLevel.Is(Configuration.GetValue("logging:level", LogEventLevel.Information))
                    .WriteTo.Console(theme: LoggerTheme.Lunar, outputTemplate: Configuration.GetValue("logging:format", "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));

                // Allow specific namespace log level overrides, which allows us to hush output from things like the database basic SELECT queries on the Information level.
                foreach (IConfigurationSection logOverride in Configuration.GetSection("logging:overrides").GetChildren())
                {
                    loggerConfiguration.MinimumLevel.Override(logOverride.Key, Enum.Parse<LogEventLevel>(logOverride.Value));
                }

                loggerConfiguration.WriteTo.File($"logs/{DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd' 'HH'_'mm'_'ss", CultureInfo.InvariantCulture)}.log", rollingInterval: Configuration.GetValue("logging:rolling_interval", RollingInterval.Day), outputTemplate: Configuration.GetValue("logging:format", "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));
                Log.Logger = loggerConfiguration.CreateLogger().ForContext<Server>();

                loggingBuilder.ClearProviders();

                if (!Configuration.GetValue("logging:disabled", false))
                {
                    loggingBuilder.AddSerilog(Log.Logger, dispose: true);
                }
            });

            // TODO: When either the plugin API system is implemented, or a complicated server config arises, add database support. See [issue #1](https://github.com/Forsaken-Borders/Moonlight/issues/1) for more information.

            // I considered injecting a CancellationToken singleton here, which would be fired when Ctrl+C is pressed. After chit-chatting with Velvet, I was convinced that it'd be better to just create a Server Shutdown event instead. Internal classes will use the CancellationToken, while plugins will use the event.
            ServiceProvider = services.BuildServiceProvider();
            Logger = Log.Logger;
            CancellationTokenSource cancellationTokenSource = new();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
                Logger.Information("Shutdown requested! Shutting down...");
            };

            SixLabors.ImageSharp.Configuration.Default.ImageFormatsManager.SetEncoder(PngFormat.Instance, new PngEncoder()
            {
                CompressionLevel = PngCompressionLevel.BestCompression
            });

            Logger.Information("Server started!");
            try
            {
                await new ServerListener().StartAsync(cancellationTokenSource.Token);
            }
            catch (TaskCanceledException) { } // Silently ignore.
        }
    }
}
