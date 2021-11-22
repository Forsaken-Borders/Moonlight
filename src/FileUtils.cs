using System;
using System.IO;
using System.Reflection;
using Serilog;

namespace Moonlight;

public static class FileUtils
{
    public static string GetConfigPath() => Directory.GetCurrentDirectory() + "/res/";

    public static void CreateDefaultConfig()
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
                Log.Warning("A config file did not exist, and one couldn't be created. Unless environment variables or command line arguments are set, default values will be used.");
            }
        }

        if (!File.Exists(resFolder + "config.json") || File.ReadAllText(resFolder + "config.json") == string.Empty)
        {
            FileStream configFile = null;
            try
            {
                configFile = File.Open(resFolder + "config.json", FileMode.Create, FileAccess.Write, FileShare.Read);
            }
            catch (Exception)
            {
                Log.Warning("A config file did not exist, and one couldn't be created. Unless environment variables or command line arguments are set, default values will be used.");
            }
            StreamReader reader = new(Assembly.GetAssembly(typeof(Server)).GetManifestResourceStream("Moonlight.res.config.json"));
            byte[] buffer = new byte[reader.BaseStream.Length];
            reader.BaseStream.Read(buffer, 0, buffer.Length);
            configFile.Write(buffer);
            configFile.Dispose();
        }

        Log.Information("Config file created, please fill it out when you get the chance.");
    }
}
