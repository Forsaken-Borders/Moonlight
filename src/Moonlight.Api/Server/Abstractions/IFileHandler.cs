using Moonlight.Api.Server.Enums;
using System.IO.Compression;

namespace Moonlight.Api.Server.Abstractions
{
    public interface IFileHandler<T>
    {
        /// <summary>
        /// Gets the config directory for the plugin. If a filename is specified, that file will be returned.
        /// </summary>
        /// <param name="filename">(Optional) The config file to grab.</param>
        /// <returns>An absolute filepath to the plugin's config directory or config file.</returns>
        string GetConfigPath(string? filename = null);

        /// <summary>
        /// Gets the cache directory, used for temporary file manipulation (such as downloading files).
        /// </summary>
        /// <returns>An absolute filepath to the plugin's cache directory or cache file.</returns>
        string GetCachePath(string? filename = null);

        /// <summary>
        /// Gets the log directory, used for storing warnings, errors or a large amount of information launched by a user.
        /// </summary>
        /// <param name="mainLog"></param>
        /// <param name="logName"></param>
        /// <returns>An absolute filepath to the plugin's log directory or log file.</returns>
        string GetLogPath(bool mainLog = true, string? logName = null);

        /// <summary>
        /// Decompresses a file located in your cache directory. Autodetects the compression type. Supported types vary, however zip, tar and tar.gz will always be supported.
        /// </summary>
        /// <param name="filename">The name of the file to decompress, located in your cache directory.</param>
        /// <returns>An absolute filepath to the decompressed file.</returns>
        string Decompress(string filename);

        /// <summary>
        /// Compresses a list of files into a single file.
        /// </summary>
        /// <param name="compressionType">Which type of compression to use.</param>
        /// <param name="compressionLevel">The compression level to use.</param>
        /// <param name="filenames">The list of files to compress.</param>
        /// <returns>An absolute filepath to the compressed file, located in the plugin's cache directory.</returns>
        string Compress(CompressionType compressionType = CompressionType.TarGzip, CompressionLevel compressionLevel = CompressionLevel.SmallestSize, params string[] filenames);

        /// <summary>
        /// Loads a plugin from the plugin directory.
        /// </summary>
        /// <param name="pluginName">The plugin to load, defined by <see cref="Plugin.Name"/>.</param>
        /// <returns>A loaded plugin.</returns>
        Plugin LoadPlugin(string pluginName);

        /// <summary>
        /// The plugin to unload. Should only be used by plugin managers, or when the <see cref="Plugin.Reload"/> and <see cref="Plugin.Unload"/> methods are not reliably functioning.
        /// </summary>
        /// <param name="plugin">The plugin to unload.</param>
        void UnloadPlugin(Plugin plugin);
    }
}