using System.IO.Compression;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace Moonlight.Api.Networking
{
    public class MinecraftStream : Stream
    {
        public Stream BaseStream { get; init; }
        public bool EncryptionEnabled { get; private set; }
        public byte[] EncryptionKey { get; private set; } = Array.Empty<byte>();
        public bool CompressionEnabled { get; private set; }
        public CompressionLevel CompressionLevel { get; private set; } = CompressionLevel.NoCompression;

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

            if (CompressionEnabled)
            {
                ReadStream = new ZLibStream(new CipherStream(BaseStream, decryptCipher, encryptCipher), CompressionLevel, false);
                WriteStream = new CipherStream(new ZLibStream(BaseStream, CompressionLevel, false), decryptCipher, encryptCipher);
            }
            else
            {
                ReadStream = new CipherStream(BaseStream, decryptCipher, encryptCipher);
                WriteStream = new CipherStream(BaseStream, decryptCipher, encryptCipher);
            }

            EncryptionEnabled = true;
            EncryptionKey = key;
        }

        public virtual void EnableCompression(CompressionLevel compressionLevel = CompressionLevel.SmallestSize)
        {
            if (compressionLevel == CompressionLevel.NoCompression)
            {
                if (EncryptionEnabled)
                {
                    CompressionEnabled = false;
                    CompressionLevel = CompressionLevel.NoCompression;
                    EnableEncryption(EncryptionKey);
                }
                else
                {
                    ReadStream = BaseStream;
                    WriteStream = BaseStream;
                }
            }
            else
            {
                if (EncryptionEnabled)
                {
                    CompressionEnabled = true;
                    CompressionLevel = compressionLevel;
                    EnableEncryption(EncryptionKey);
                }
                else
                {
                    ReadStream = new ZLibStream(BaseStream, compressionLevel, false);
                    WriteStream = new ZLibStream(BaseStream, compressionLevel, false);
                }
            }
        }
    }
}