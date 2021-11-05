using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Moonlight.Types
{
    public class AesStream : Stream
    {
        private CryptoStream ReadStream { get; init; }
        private CryptoStream WriteStream { get; init; }

        public override bool CanRead => ReadStream.CanRead;
        public override bool CanSeek => ReadStream.CanSeek;
        public override bool CanWrite => WriteStream.CanWrite;
        public override long Length => ReadStream.Length;
        public override long Position { get => ReadStream.Position; set => ReadStream.Position = value; }
        public override bool CanTimeout => ReadStream.CanTimeout;
        public override int ReadTimeout { get => ReadStream.ReadTimeout; set => ReadStream.ReadTimeout = value; }
        public override int WriteTimeout { get => WriteStream.WriteTimeout; set => WriteStream.WriteTimeout = value; }

        public AesStream(Stream baseStream, byte[] sharedSecret)
        {
            Aes aes = Aes.Create();
            aes.Key = sharedSecret;
            aes.IV = sharedSecret;
            ReadStream = new CryptoStream(baseStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            WriteStream = new CryptoStream(baseStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => ReadStream.BeginRead(buffer, offset, count, callback, state);
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => WriteStream.BeginWrite(buffer, offset, count, callback, state);
        public override int Read(Span<byte> buffer) => ReadStream.Read(buffer);
        public override int Read(byte[] buffer, int offset, int count) => ReadStream.Read(buffer, offset, count);
        public override int ReadByte() => ReadStream.ReadByte();
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => ReadStream.ReadAsync(buffer, cancellationToken);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) => ReadStream.ReadAsync(buffer, offset, count, cancellationToken);
        public override void Write(byte[] buffer, int offset, int count) => WriteStream.Write(buffer, offset, count);
        public override void Write(ReadOnlySpan<byte> buffer) => WriteStream.Write(buffer);
        public override void WriteByte(byte value) => WriteStream.WriteByte(value);
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) => WriteStream.WriteAsync(buffer, offset, count, cancellationToken);
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => WriteStream.WriteAsync(buffer, cancellationToken);
        public override void Flush() => WriteStream.Flush();
        public override long Seek(long offset, SeekOrigin origin) => ReadStream.Seek(offset, origin);
        public override void SetLength(long value) => ReadStream.SetLength(value);
        public override void Close()
        {
            ReadStream.Close();
            WriteStream.Close();
        }

        public override async ValueTask DisposeAsync()
        {
            await ReadStream.DisposeAsync();
            await WriteStream.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}