using System;
using System.Globalization;
using System.Runtime.CompilerServices;
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

namespace Moonlight
{
    internal class Program
    {
        internal static IConfiguration Configuration { get; set; }
        internal static IServiceProvider ServiceProvider { get; set; }
        internal static Serilog.ILogger Logger { get; set; }
        internal static CancellationTokenSource CancellationTokenSource { get; set; } = new();

        public static async Task Main(string[] args)
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                CancellationTokenSource.Cancel();
                Logger.Information("Shutdown requested! Shutting down...");
            };

            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.Sources.Clear();
            configurationBuilder.AddYamlFile(GetSourceFilePathName() + "../../../res/config.yml", true, true);
            configurationBuilder.AddJsonFile(GetSourceFilePathName() + "../../../res/config.json", true, true);
            configurationBuilder.AddEnvironmentVariables("MOONLIGHT_");
            configurationBuilder.AddCommandLine(args);
            Configuration = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(Configuration);
            services.AddLogging(loggingBuilder =>
            {
                LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                    .Enrich.WithThreadId()
                    .MinimumLevel.Is(Configuration.GetValue("logging:level", LogEventLevel.Information))
                    .WriteTo.Console(theme: LoggerTheme.Lunar, outputTemplate: Configuration.GetValue("logging:format", "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));
                foreach (IConfigurationSection logOverride in Configuration.GetSection("logging:overrides").GetChildren())
                {
                    loggerConfiguration.MinimumLevel.Override(logOverride.Key, Enum.Parse<LogEventLevel>(logOverride.Value));
                }

                loggerConfiguration.WriteTo.File($"logs/{DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd' 'HH'_'mm'_'ss", CultureInfo.InvariantCulture)}.log", rollingInterval: Configuration.GetValue<RollingInterval>("logging:rolling_interval"), outputTemplate: Configuration.GetValue("logging:format", "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));
                Log.Logger = loggerConfiguration.CreateLogger();

                if (Configuration.GetValue<bool>("logging:disabled"))
                {
                    loggingBuilder.ClearProviders();
                }
                else
                {
                    loggingBuilder.AddSerilog(Log.Logger, dispose: true);
                }
            });

            ServiceProvider = services.BuildServiceProvider();
            Logger = Log.Logger;

            await new ServerListener().StartAsync(CancellationTokenSource.Token);
        }

        private static string GetSourceFilePathName([CallerFilePath] string callerFilePath = null) => string.IsNullOrEmpty(callerFilePath) ? "" : callerFilePath;
    }
}
