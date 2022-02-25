using Moonlight.Api.Server.Abstractions;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Writers;
using System;
using System.IO;
using System.IO.Compression;

namespace Moonlight.Server.Objects
{
    public class FileHandler : IFileHandler<Program>
    {
        public void CreateDefaultConfig()
        {
            string configPath = Path.Join(GetConfigPath(), "config.json");
            if (File.Exists(configPath) || !string.IsNullOrWhiteSpace(File.ReadAllText(configPath)))
            {
                return;
            }

            using FileStream fileStream = File.Open(configPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            using FileStream? configFileStream = typeof(Program).Assembly.GetFile("config.json");
            if (configFileStream == null)
            {
                throw new FileNotFoundException("Could not find the default config file that's supposed to be embedded into Moonlight.");
            }
            configFileStream.CopyTo(fileStream);
            configFileStream.Close();
            fileStream.Close();
        }
        public string GetCachePath(string? filename = null) => Path.Join(Directory.GetCurrentDirectory(), "cache/");
        public string GetConfigPath(string? filename = null) => Path.Join(Directory.GetCurrentDirectory(), "res/");
        public string GetLogPath(bool mainLog = true, string? logName = null) => Path.Join(Directory.GetCurrentDirectory(), "logs/");

        public string Compress(Api.Server.Enums.CompressionType compressionType = Api.Server.Enums.CompressionType.TarGzip, CompressionLevel compressionLevel = CompressionLevel.SmallestSize, string? compressedFileName = null, params string[] filenames)
        {
            // FIXME: This could be improved upon to support all compression types that SharpCompress supports. Free PRs welcome!
            switch (compressionType)
            {
                case Api.Server.Enums.CompressionType.Zip:
                    compressedFileName ??= Path.Join(GetCachePath(), "archive.zip");
                    using (ZipArchive? zip = File.Exists(compressedFileName) ? new ZipArchive(File.Open(compressedFileName, FileMode.Open), ZipArchiveMode.Update, false) : new ZipArchive(File.Create(compressedFileName), ZipArchiveMode.Create, false))
                    {
                        foreach (string? filename in filenames)
                        {
                            zip.CreateEntryFromFile(filename, Path.GetFileName(filename));
                        }
                    }
                    break;
                case Api.Server.Enums.CompressionType.Tar:
                    compressedFileName ??= Path.Join(GetCachePath(), "archive.tar");
                    using (TarArchive? tar = TarArchive.Open(compressedFileName))
                    {
                        foreach (string? filename in filenames)
                        {
                            tar.AddEntry(filename, filename);
                        }
                        tar.SaveTo(compressedFileName, new WriterOptions(CompressionType.None));
                    }
                    break;
                case Api.Server.Enums.CompressionType.TarGzip:
                    compressedFileName ??= Path.Join(GetCachePath(), "archive.tar.gz");
                    using (TarArchive? tar = TarArchive.Open(compressedFileName))
                    {
                        foreach (string? filename in filenames)
                        {
                            tar.AddEntry(filename, filename);
                        }
                        tar.SaveTo(compressedFileName, new WriterOptions(CompressionType.GZip));
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return compressedFileName;
        }

        public bool Decompress(string filename, out string outputDirectory)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException("Filename cannot be null or whitespace.", nameof(filename));
            }
            else if (!File.Exists(filename))
            {
                throw new FileNotFoundException("File not found.", filename);
            }

            try
            {
                outputDirectory = Path.Join(GetCachePath(), Path.GetFileNameWithoutExtension(filename));
                using IArchive archive = ArchiveFactory.Open(filename);
                archive.WriteToDirectory(outputDirectory, new ExtractionOptions() { Overwrite = true });
                return true;
            }
            catch (InvalidOperationException)
            {
                throw;
            }

        }

        public Plugin LoadPlugin(string pluginName) => throw new NotImplementedException();
        public void UnloadPlugin(Plugin plugin) => throw new NotImplementedException();
    }
}