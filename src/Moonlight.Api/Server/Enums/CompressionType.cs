namespace Moonlight.Api.Server.Enums
{
    public enum CompressionType
    {
        /// <summary>
        /// Most widely used, compatibile with all operating systems.
        /// </summary>
        Zip,

        /// <summary>
        /// Compatible with Linux and MacOS, used to put all the files into one with no compression.
        /// </summary>
        Tar,

        /// <summary>
        /// Compatible with Linux and MacOS, has slightly better compression than <see cref="Zip"/>.
        /// </summary>
        TarGzip
    }
}