using System.Globalization;
using System.Reflection;
using Serilog;

namespace Moonlight.Api.Types
{
    public class FileUtils<T>
    {
        public string PluginName { get; } = string.Empty;
        public Type Type { get; }
        private ILogger Logger { get; }

        public FileUtils(string pluginName, ILogger? logger = null)
        {
            // We want the server config to be in the `/res` directory instead of the `/res/Moonlight` directory to prevent confusion for malicious plugins which try to copy Moonlight's file structure.
            // FIXME: I don't like the second check, since there could be tons of FileUtils classes being initiated for many different plugins, but everytime it's done, it checks to see if the type belongs to the server.
            // What should be done is to have a constructor available only to Moonlight, but since `Moonlight` and `Moonlight.Api` are different namespaces/assemblies, we can't use the `internal` modifier.
            if (string.IsNullOrWhiteSpace(pluginName) && (!typeof(T).Namespace?.StartsWith("Moonlight", true, CultureInfo.InvariantCulture) ?? false))
            {
                throw new ArgumentException("The pluginName cannot be null or empty!", nameof(pluginName));
            }
            PluginName = pluginName;
            Logger = logger ?? Log.Logger;
            Type = typeof(T);
        }

        public string GetConfigPath() => Path.Combine(Directory.GetCurrentDirectory(), "res/", PluginName);

        public void CreateDefaultConfig()
        {
            string resFolder = GetConfigPath();
            if (!Directory.Exists(resFolder))
            {
                try
                {
                    Directory.CreateDirectory(resFolder);
                }
                catch (Exception)
                {
                    Logger.Warning("Failed to create the resource directory. Unless environment variables or command line arguments are set, default values will be used.");
                    return;
                }
            }

            if (!File.Exists(resFolder + "config.json") || File.ReadAllText(resFolder + "config.json") == string.Empty)
            {
                FileStream configFile;
                try
                {
                    configFile = File.Open(resFolder + "config.json", FileMode.Create, FileAccess.Write, FileShare.Read);
                }
                catch (Exception)
                {
                    Logger.Warning("Failed to read or create the config file. Unless environment variables or command line arguments are set, default values will be used.");
                    return;
                }
                StreamReader reader = new(Assembly.GetAssembly(Type)!.GetManifestResourceStream("config.json") ?? throw new InvalidOperationException("config.json was not compiled into the plugin, unable to create it."));
                byte[] buffer = new byte[reader.BaseStream.Length];
                reader.BaseStream.Read(buffer, 0, buffer.Length);
                configFile.Write(buffer);
                configFile.Dispose();
            }

            Log.Information("Config file created, please fill it out when you get the chance.");
        }
    }
}