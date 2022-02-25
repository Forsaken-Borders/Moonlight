using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System.IO.Compression;

namespace Moonlight.Api.Minecraft.Objects.Networking
{
    public class MinecraftStream : Stream
    {
        public Stream BaseStream { get; init; }
        public byte[] EncryptionKey { get; private set; } = Array.Empty<byte>();
        public CompressionLevel CompressionLevel { get; private set; } = CompressionLevel.NoCompression;
        public bool EncryptionEnabled => ReadStream is CipherStream;
        public bool CompressionEnabled => CompressionLevel != CompressionLevel.NoCompression;

        private Stream ReadStream { get; set; }
        private Stream WriteStream { get; set; }

        public MinecraftStream(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            BaseStream = stream;
            ReadStream = stream;
            WriteStream = stream;
        }

        public override bool CanRead => BaseStream.CanRead;
        public override bool CanSeek => BaseStream.CanSeek;
        public override bool CanWrite => BaseStream.CanWrite;
        public override long Length => BaseStream.Length;
        public override long Position { get => BaseStream.Position; set => BaseStream.Position = value; }

        public override void Flush() => BaseStream.Flush();
        public override long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);
        public override void SetLength(long value) => BaseStream.SetLength(value);
        public override int Read(byte[] buffer, int offset, int count) => ReadStream.Read(buffer, offset, count);
        public override void Write(byte[] buffer, int offset, int count) => WriteStream.Write(buffer, offset, count);

        public virtual void EnableEncryption(byte[] key)
        {
            BufferedBlockCipher? encryptCipher = new(new CfbBlockCipher(new AesEngine(), 8));
            encryptCipher.Init(true, new ParametersWithIV(new KeyParameter(key), key, 0, 16));

            BufferedBlockCipher? decryptCipher = new(new CfbBlockCipher(new AesEngine(), 8));
            decryptCipher.Init(false, new ParametersWithIV(new KeyParameter(key), key, 0, 16));

            ReadStream = new CipherStream(BaseStream, decryptCipher, encryptCipher);
            WriteStream = new CipherStream(BaseStream, decryptCipher, encryptCipher);
            EncryptionKey = key;
        }

        /// <summary>
        /// Enables compression after encryption is enabled. Set the compression level to <see cref="CompressionLevel.NoCompression"/> to disable compression.
        /// </summary>
        /// <param name="compressionLevel">The level of compression to set. <see cref="CompressionLevel.NoCompression"/> disables compression.</param>
        public virtual void EnableCompression(CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            if (!EncryptionEnabled)
            {
                throw new InvalidOperationException("Encryption must be enabled before compression.");
            }

            BufferedBlockCipher? encryptCipher = new(new CfbBlockCipher(new AesEngine(), 8));
            encryptCipher.Init(true, new ParametersWithIV(new KeyParameter(EncryptionKey), EncryptionKey, 0, 16));
            BufferedBlockCipher? decryptCipher = new(new CfbBlockCipher(new AesEngine(), 8));
            decryptCipher.Init(false, new ParametersWithIV(new KeyParameter(EncryptionKey), EncryptionKey, 0, 16));

            CompressionLevel = compressionLevel;
            ReadStream = new CipherStream(new ZLibStream(BaseStream, CompressionLevel, false), decryptCipher, encryptCipher); // decrypt, decompress
            WriteStream = new ZLibStream(new CipherStream(BaseStream, decryptCipher, encryptCipher), CompressionLevel, false); // compress, encrypt
        }
    }
}