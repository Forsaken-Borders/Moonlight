using System;
using System.Globalization;
using System.Reflection;
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

            serviceCollection.AddSingleton(typeof(AsyncServerEvent<>));
            serviceCollection.AddSingleton<ServerConfiguration>();
            serviceCollection.AddSingleton<Server>();

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

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

                    object asyncServerEvent = serviceProvider.GetRequiredService(typeof(AsyncServerEvent<>).MakeGenericType(parameters[0].ParameterType));
                    MethodInfo addPreHandler = asyncServerEvent.GetType().GetMethod("AddPreHandler") ?? throw new InvalidOperationException("Could not find the method 'AddPreHandler' in 'AsyncServerEvent<>'.");
                    MethodInfo addPostHandler = asyncServerEvent.GetType().GetMethod("AddPostHandler") ?? throw new InvalidOperationException("Could not find the method 'AddPostHandler' in 'AsyncServerEvent<>'.");
                    if (method.ReturnType == typeof(ValueTask<bool>))
                    {
                        if (method.IsStatic)
                        {
                            // Invoke void AddPreHandler(AsyncServerEventPreHandler<TEventArgs> handler, AsyncServerEventPriority priority = AsyncServerEventPriority.Normal)
                            addPreHandler.Invoke(asyncServerEvent, [
                                // Create a delegate of the method
                                Delegate.CreateDelegate(typeof(AsyncServerEventPreHandler<>).MakeGenericType(parameters[0].ParameterType), method),
                                // Normal priority
                                AsyncServerEventPriority.Normal
                            ]);
                        }
                        else
                        {
                            // Invoke void AddPreHandler(AsyncServerEventPreHandler<TEventArgs> handler, AsyncServerEventPriority priority = AsyncServerEventPriority.Normal)
                            addPreHandler.Invoke(asyncServerEvent, [
                                // Create a delegate of the method
                                Delegate.CreateDelegate(typeof(AsyncServerEventPreHandler<>).MakeGenericType(parameters[0].ParameterType), ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, type), method),
                                // Normal priority
                                AsyncServerEventPriority.Normal
                            ]);
                        }
                    }
                    else
                    {
                        if (method.IsStatic)
                        {
                            // Invoke void AddPostHandler(AsyncServerEventHandler<TEventArgs> handler, AsyncServerEventPriority priority = AsyncServerEventPriority.Normal)
                            addPostHandler.Invoke(asyncServerEvent, [
                                // Create a delegate of the method
                                Delegate.CreateDelegate(typeof(AsyncServerEventHandler<>).MakeGenericType(parameters[0].ParameterType), method),
                                // Normal priority
                                AsyncServerEventPriority.Normal
                            ]);
                        }
                        else
                        {
                            // Invoke void AddPostHandler(AsyncServerEventHandler<TEventArgs> handler, AsyncServerEventPriority priority = AsyncServerEventPriority.Normal)
                            addPostHandler.Invoke(asyncServerEvent, [
                                // Create a delegate of the method
                                Delegate.CreateDelegate(typeof(AsyncServerEventHandler<>).MakeGenericType(parameters[0].ParameterType), ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, type), method),
                                // Normal priority
                                AsyncServerEventPriority.Normal
                            ]);
                        }
                    }
                }
            }

            Server server = serviceProvider.GetRequiredService<Server>();
            await server.StartAsync();
        }
    }
}
