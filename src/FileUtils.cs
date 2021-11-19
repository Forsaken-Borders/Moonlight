using System.IO;

namespace Moonlight
{
    public class FileUtils
    {
        public static string GetConfigPath() => Directory.GetCurrentDirectory() + "/res/";
    }
}