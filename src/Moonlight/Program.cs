using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moonlight.Api;
using Moonlight.Api.Events;
using Moonlight.Api.Net;
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

            serviceCollection.AddSingleton((serviceProvider) =>
            {
                PacketReaderFactory packetReaderFactory = new(serviceProvider.GetRequiredService<ILoggerFactory>());
                packetReaderFactory.AddDefaultPacketDeserializers();
                return packetReaderFactory;
            });

            serviceCollection.AddSingleton<AsyncServerEventContainer>();
            serviceCollection.AddKeyedSingleton("Moonlight.Handshake", (serviceProvider, key) =>
            {
                PacketReaderFactory packetReaderFactory = ActivatorUtilities.CreateInstance<PacketReaderFactory>(serviceProvider);
                packetReaderFactory.AddDefaultPacketDeserializers();
                packetReaderFactory.Prepare();
                return packetReaderFactory;
            });

            serviceCollection.AddKeyedSingleton("Moonlight.Play", (serviceProvider, key) =>
            {
                PacketReaderFactory packetReaderFactory = ActivatorUtilities.CreateInstance<PacketReaderFactory>(serviceProvider);
                packetReaderFactory.AddDefaultPacketDeserializers();
                packetReaderFactory.Prepare();
                return packetReaderFactory;
            });

            serviceCollection.AddSingleton<ServerConfiguration>();
            serviceCollection.AddSingleton<Server>();

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            AsyncServerEventContainer asyncServerEventContainer = serviceProvider.GetRequiredService<AsyncServerEventContainer>();

            // Register all event handlers
            foreach (Type type in typeof(Program).Assembly.GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length != 1 || !parameters[0].ParameterType.IsAssignableTo(typeof(AsyncServerEventArgs)))
                    {
                        continue;
                    }
                    else if (method.ReturnType == typeof(ValueTask<bool>))
                    {
                        Delegate genericHandler = method.IsStatic
                            ? Delegate.CreateDelegate(typeof(AsyncServerEventPreHandler<>).MakeGenericType(parameters[0].ParameterType), method)
                            : Delegate.CreateDelegate(typeof(AsyncServerEventPreHandler<>).MakeGenericType(parameters[0].ParameterType), ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, type), method);

                        asyncServerEventContainer.AddPreHandler(parameters[0].ParameterType, Unsafe.As<AsyncServerEventPreHandler>(genericHandler), AsyncServerEventPriority.Normal);
                    }
                    else
                    {
                        Delegate genericHandler = method.IsStatic
                            ? Delegate.CreateDelegate(typeof(AsyncServerEventHandler<>).MakeGenericType(parameters[0].ParameterType), method)
                            : Delegate.CreateDelegate(typeof(AsyncServerEventHandler<>).MakeGenericType(parameters[0].ParameterType), ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, type), method);

                        asyncServerEventContainer.AddPostHandler(parameters[0].ParameterType, Unsafe.As<AsyncServerEventHandler>(genericHandler), AsyncServerEventPriority.Normal);
                    }
                }
            }

            Server server = serviceProvider.GetRequiredService<Server>();
            await server.StartAsync();
        }
    }
}
