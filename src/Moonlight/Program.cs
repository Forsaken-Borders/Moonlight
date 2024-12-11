using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moonlight.Api;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using SerilogLoggerConfiguration = Serilog.LoggerConfiguration;

namespace Moonlight
{
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(serviceProvider =>
            {
                ConfigurationBuilder configurationBuilder = new();
                configurationBuilder.Sources.Clear();
                configurationBuilder.AddJsonFile("config.json", true, true);
#if DEBUG
                // If the program is running in debug mode, add the debug config file
                configurationBuilder.AddJsonFile("config.debug.json", true, true);
#endif
                configurationBuilder.AddEnvironmentVariables("TOMOE__");
                configurationBuilder.AddCommandLine(args);

                return configurationBuilder.Build();
            });

            serviceCollection.AddLogging(logging =>
            {
                IServiceProvider serviceProvider = logging.Services.BuildServiceProvider();
                LoggerConfiguration serverConfiguration = serviceProvider.GetRequiredService<IConfiguration>().GetSection("Logging").Get<LoggerConfiguration>() ?? new();

                SerilogLoggerConfiguration serilogLoggerConfiguration = new();
                serilogLoggerConfiguration.MinimumLevel.Is(serverConfiguration.LogLevel);
                serilogLoggerConfiguration.WriteTo.Console(
                    formatProvider: CultureInfo.InvariantCulture,
                    outputTemplate: serverConfiguration.Format,
                    theme: AnsiConsoleTheme.Code
                );

                serilogLoggerConfiguration.WriteTo.File(
                    formatProvider: CultureInfo.InvariantCulture,
                    path: $"{serverConfiguration.Path}/{DateTime.Now.ToUniversalTime().ToString(serverConfiguration.FileName, CultureInfo.InvariantCulture)}-.log",
                    rollingInterval: serverConfiguration.RollingInterval,
                    outputTemplate: serverConfiguration.Format
                );

                // Sometimes the user/dev needs more or less information about a speific part of the bot
                // so we allow them to override the log level for a specific namespace.
                if (serverConfiguration.Overrides.Count > 0)
                {
                    foreach ((string key, LogEventLevel value) in serverConfiguration.Overrides)
                    {
                        serilogLoggerConfiguration.MinimumLevel.Override(key, value);
                    }
                }

                logging.AddSerilog(serilogLoggerConfiguration.CreateLogger());
            });

            serviceCollection.AddSingleton<ServerConfiguration>();
            serviceCollection.AddSingleton<Server>();

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            Server server = serviceProvider.GetRequiredService<Server>();
            await server.StartAsync();
        }
    }
}
