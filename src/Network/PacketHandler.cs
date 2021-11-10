using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moonlight.Network.Packets;
using Moonlight.Types;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;

/*
 Based off of the following files:
 - https://github.com/ObsidianMC/Obsidian/blob/958bf6e97dde05dd93019bdd0d77570cbb0520b8/Obsidian/Net/MinecraftStream.cs
 - https://github.com/ObsidianMC/Obsidian/blob/958bf6e97dde05dd93019bdd0d77570cbb0520b8/Obsidian/Net/MinecraftStream.Reading.cs
 - https://github.com/ObsidianMC/Obsidian/blob/958bf6e97dde05dd93019bdd0d77570cbb0520b8/Obsidian/Net/MinecraftStream.Writing.cs
 Thank you to the ObsidianMC team!
*/

namespace Moonlight.Network
{
    public class PacketHandler : IDisposable
    {
        public Stream Stream { get; private set; }
        public CancellationToken CancellationToken { get; init; }
        public AsymmetricCipherKeyPair Keys { get; private set; }

        public PacketHandler(Stream stream, CancellationToken cancellationToken = new())
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));
            Stream = stream;
            CancellationToken = cancellationToken;
        }

        public PacketHandler(byte[] data, CancellationToken cancellationToken = new())
        {
            ArgumentNullException.ThrowIfNull(data, nameof(data));

            Stream = new MemoryStream(data);
            CancellationToken = cancellationToken;
        }

        // Vaguely based off of https://github.com/ObsidianMC/Obsidian/blob/755fc9ce44197a76f186ca555eaa41b5fd9efbd1/Obsidian/Net/PacketCryptography.cs#L23-L45
        public void GenerateKeys()
        {
            RsaKeyPairGenerator rsaKeyPairGenerator = new();
            rsaKeyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 1024));
            Keys = rsaKeyPairGenerator.GenerateKeyPair();
        }

        public void EnableEncryption(byte[] sharedSecret) => Stream = new AesStream(Stream, sharedSecret);

        // An optimization would be to copy what the BCL does, which is essentially -> ReadAsync().GetAwaiter().GetResult()
        public byte ReadUnsignedByte()
        {
            byte[] totalLength = new byte[1];
            Stream.Read(totalLength);
            return totalLength[0];
        }

        public async Task<byte> ReadUnsignedByteAsync()
        {
            byte[] totalLength = new byte[1];
            await Stream.ReadAsync(totalLength, CancellationToken);
            return totalLength[0];
        }

        public sbyte ReadByte() => (sbyte)ReadUnsignedByte();

        public async Task<sbyte> ReadByteAsync() => (sbyte)await ReadUnsignedByteAsync();

        public bool ReadBoolean()
        {
            return (int)ReadUnsignedByte() switch
            {
                0x00 => false,
                0x01 => true,
                _ => throw new ArgumentOutOfRangeException("Byte returned by stream is out of range (0x00 or 0x01)", nameof(TcpClient))
            };
        }

        public async Task<bool> ReadBooleanAsync()
        {
            return (int)await ReadUnsignedByteAsync() switch
            {
                0x00 => false,
                0x01 => true,
                _ => throw new ArgumentOutOfRangeException("Byte returned by stream is out of range (0x00 or 0x01)", nameof(TcpClient))
            };
        }

        public ushort ReadUnsignedShort()
        {
            byte[] buffer = new byte[2];
            Stream.Read(buffer);
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }

        public async Task<ushort> ReadUnsignedShortAsync()
        {
            byte[] buffer = new byte[2];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }

        public short ReadShort()
        {
            byte[] buffer = new byte[2];
            Stream.Read(buffer);
            return BinaryPrimitives.ReadInt16BigEndian(buffer);
        }

        public async Task<short> ReadShortAsync()
        {
            byte[] buffer = new byte[2];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadInt16BigEndian(buffer);
        }

        public int ReadInt()
        {
            byte[] buffer = new byte[4];
            Stream.Read(buffer);
            return BinaryPrimitives.ReadInt32BigEndian(buffer);
        }

        public async Task<int> ReadIntAsync()
        {
            byte[] buffer = new byte[4];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadInt32BigEndian(buffer);
        }

        public long ReadLong()
        {
            byte[] buffer = new byte[8];
            Stream.Read(buffer);
            return BinaryPrimitives.ReadInt64BigEndian(buffer);
        }

        public async Task<long> ReadLongAsync()
        {
            byte[] buffer = new byte[8];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadInt64BigEndian(buffer);
        }

        public ulong ReadUnsignedLong()
        {
            byte[] buffer = new byte[8];
            Stream.Read(buffer);
            return BinaryPrimitives.ReadUInt64BigEndian(buffer);
        }

        public async Task<ulong> ReadUnsignedLongAsync()
        {
            byte[] buffer = new byte[8];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadUInt64BigEndian(buffer);
        }

        public float ReadFloat()
        {
            byte[] buffer = new byte[4];
            Stream.Read(buffer);
            return BinaryPrimitives.ReadSingleBigEndian(buffer);
        }

        public async Task<float> ReadFloatAsync()
        {
            byte[] buffer = new byte[4];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadSingleBigEndian(buffer);
        }

        public double ReadDouble()
        {
            byte[] buffer = new byte[8];
            Stream.Read(buffer);
            return BinaryPrimitives.ReadDoubleBigEndian(buffer);
        }

        public async Task<double> ReadDoubleAsync()
        {
            byte[] buffer = new byte[8];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadDoubleBigEndian(buffer);
        }

        public string ReadString(int maxLength = short.MaxValue)
        {
            int length = ReadVarInt();
            byte[] buffer = new byte[length];
            if (BitConverter.IsLittleEndian) //Why? This isn't present in any other method.
            {
                Array.Reverse(buffer);
            }
            Stream.Read(buffer);

            string value = Encoding.UTF8.GetString(buffer);
            return maxLength > 0 && value.Length > maxLength
                ? throw new InvalidOperationException($"String ({value.Length}) exceeded maximum length ({maxLength})")
                : value;
        }

        public async Task<string> ReadStringAsync(int maxLength = short.MaxValue)
        {
            int length = await ReadVarIntAsync();
            byte[] buffer = new byte[length];
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            await Stream.ReadAsync(buffer, CancellationToken);

            string value = Encoding.UTF8.GetString(buffer);
            return maxLength > 0 && value.Length > maxLength
                ? throw new InvalidOperationException($"String ({value.Length}) exceeded maximum length ({maxLength})")
                : value;
        }

        public int ReadVarInt()
        {
            int numRead = 0;
            int result = 0;
            byte read;
            do
            {
                read = ReadUnsignedByte();
                int value = read & 0b01111111;
                result |= value << (7 * numRead);

                numRead++;
                if (numRead > 5)
                {
                    throw new InvalidOperationException("VarInt is too big");
                }
            } while ((read & 0b10000000) != 0);

            return result;
        }

        public async Task<int> ReadVarIntAsync()
        {
            int numRead = 0;
            int result = 0;
            byte read;
            do
            {
                read = await ReadUnsignedByteAsync();
                int value = read & 0b01111111;
                result |= value << (7 * numRead);

                numRead++;
                if (numRead > 5)
                {
                    throw new InvalidOperationException("VarInt is too big");
                }
            } while ((read & 0b10000000) != 0);

            return result;
        }

        public byte[] ReadUInt8Array(int length = 0)
        {
            if (length == 0)
            {
                length = ReadVarInt();
            }

            byte[] result = new byte[length];
            if (length == 0)
            {
                return result;
            }

            int n = length;
            while (true)
            {
                n -= Stream.Read(result, length - n, n);
                if (n == 0)
                {
                    break;
                }
            }
            return result;
        }

        public async Task<byte[]> ReadUInt8ArrayAsync(int length = 0)
        {
            if (length == 0)
            {
                length = await ReadVarIntAsync();
            }

            byte[] result = new byte[length];
            if (length == 0)
            {
                return result;
            }

            int n = length;
            while (true)
            {
                n -= await Stream.ReadAsync(result.AsMemory(length - n, n));
                if (n == 0)
                {
                    break;
                }
            }
            return result;
        }

        public long ReadVarLong()
        {
            int numRead = 0;
            long result = 0;
            byte read;
            do
            {
                read = ReadUnsignedByte();
                int value = read & 0b01111111;
                result |= (long)value << (7 * numRead);

                numRead++;
                if (numRead > 10)
                {
                    throw new InvalidOperationException("VarLong is too big");
                }
            } while ((read & 0b10000000) != 0);

            return result;
        }

        public async Task<long> ReadVarLongAsync()
        {
            int numRead = 0;
            long result = 0;
            byte read;
            do
            {
                read = await ReadUnsignedByteAsync();
                int value = read & 0b01111111;
                result |= (long)value << (7 * numRead);

                numRead++;
                if (numRead > 10)
                {
                    throw new InvalidOperationException("VarLong is too big");
                }
            } while ((read & 0b10000000) != 0);

            return result;
        }

        public Packet ReadNextPacket()
        {
            int totalPacketLength = ReadVarInt();
            int packetId = ReadVarInt();
            int packetDataLength = totalPacketLength - packetId.GetVarIntLength();

            if (packetDataLength <= 0)
            {
                return new Packet(packetId);
            }

            byte[] packetData = new byte[packetDataLength];
            Stream.Read(packetData);

            return new Packet(packetId, packetData);
        }

        public async Task<Packet> ReadNextPacketAsync()
        {
            int totalPacketLength = await ReadVarIntAsync();
            int packetId = await ReadVarIntAsync();
            int packetDataLength = totalPacketLength - packetId.GetVarIntLength();

            if (packetDataLength <= 0)
            {
                return new Packet(packetId);
            }

            byte[] packetData = new byte[packetDataLength];
            await Stream.ReadAsync(packetData, CancellationToken);

            return new Packet(packetId, packetData);
        }

        public void WriteUnsignedByte(byte value) => Stream.WriteByte(value);
        public ValueTask WriteUnsignedByteAsync(byte value) => Stream.WriteAsync(new[] { value }, CancellationToken);
        public void WriteUnsignedBytes(byte[] values) => Stream.Write(values);
        public ValueTask WriteUnsignedBytesAsync(byte[] values) => Stream.WriteAsync(values, CancellationToken);
        public void WriteByte(sbyte value) => WriteUnsignedByte((byte)value);
        public ValueTask WriteByteAsync(sbyte value) => WriteUnsignedByteAsync((byte)value);
        public void WriteBytes(sbyte[] values) => WriteUnsignedBytes(values.Cast<byte>().ToArray());
        public ValueTask WriteBytesAsync(sbyte[] values) => WriteUnsignedBytesAsync(values.Cast<byte>().ToArray());
        public void WriteBoolean(bool value) => WriteUnsignedByte((byte)(value ? 0x01 : 0x00));
        public ValueTask WriteBooleanAsync(bool value) => WriteUnsignedByteAsync((byte)(value ? 0x01 : 0x00));

        public void WriteUnsignedShort(ushort value)
        {
            byte[] write = new byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16BigEndian(write, value);
            Stream.Write(write);
        }

        public async Task WriteUnsignedShortAsync(ushort value)
        {
            byte[] write = new byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16BigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public void WriteShort(short value)
        {
            byte[] write = new byte[sizeof(short)];
            BinaryPrimitives.WriteInt16BigEndian(write, value);
            Stream.Write(write);
        }

        public async Task WriteShortAsync(short value)
        {
            byte[] write = new byte[sizeof(short)];
            BinaryPrimitives.WriteInt16BigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public void WriteInt(int value)
        {
            byte[] write = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(write, value);
            Stream.Write(write);
        }

        public async Task WriteIntAsync(int value)
        {
            byte[] write = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public void WriteLong(long value)
        {
            byte[] write = new byte[sizeof(long)];
            BinaryPrimitives.WriteInt64BigEndian(write, value);
            Stream.Write(write);
        }

        public async Task WriteLongAsync(long value)
        {
            byte[] write = new byte[sizeof(long)];
            BinaryPrimitives.WriteInt64BigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public void WriteFloat(float value)
        {
            byte[] write = new byte[sizeof(float)];
            BinaryPrimitives.WriteSingleBigEndian(write, value);
            Stream.Write(write);
        }

        public async Task WriteFloatAsync(float value)
        {
            byte[] write = new byte[sizeof(float)];
            BinaryPrimitives.WriteSingleBigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public void WriteDouble(double value)
        {
            byte[] write = new byte[sizeof(double)];
            BinaryPrimitives.WriteDoubleBigEndian(write, value);
            Stream.Write(write);
        }

        public async Task WriteDoubleAsync(double value)
        {
            byte[] write = new byte[sizeof(double)];
            BinaryPrimitives.WriteDoubleBigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public void WriteString(string value, int maxLength = short.MaxValue)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            if (value.Length > maxLength)
            {
                throw new ArgumentException($"string ({value.Length}) exceeded maximum length ({maxLength})", nameof(value));
            }

            byte[] write = Encoding.UTF8.GetBytes(value);
            WriteVarInt(write.Length);
            Stream.Write(write);
        }

        public async Task WriteStringAsync(string value, int maxLength = short.MaxValue)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            if (value.Length > maxLength)
            {
                throw new ArgumentException($"string ({value.Length}) exceeded maximum length ({maxLength})", nameof(value));
            }

            byte[] write = Encoding.UTF8.GetBytes(value);
            await WriteVarIntAsync(write.Length);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public void WriteVarInt(int value)
        {
            uint unsigned = (uint)value;

            do
            {
                byte temp = (byte)(unsigned & 127);
                unsigned >>= 7;
                if (unsigned != 0)
                {
                    temp |= 128;
                }

                WriteUnsignedByte(temp);
            }
            while (unsigned != 0);
        }

        public async Task WriteVarIntAsync(int value)
        {
            uint unsigned = (uint)value;

            do
            {
                byte temp = (byte)(unsigned & 127);
                unsigned >>= 7;
                if (unsigned != 0)
                {
                    temp |= 128;
                }

                await WriteUnsignedByteAsync(temp);
            }
            while (unsigned != 0);
        }

        public void WriteVarLong(long value)
        {
            ulong unsigned = (ulong)value;

            do
            {
                byte temp = (byte)(unsigned & 127);
                unsigned >>= 7;
                if (unsigned != 0)
                {
                    temp |= 128;
                }

                WriteUnsignedByte(temp);
            }
            while (unsigned != 0);
        }

        public async Task WriteVarLongAsync(long value)
        {
            ulong unsigned = (ulong)value;

            do
            {
                byte temp = (byte)(unsigned & 127);
                unsigned >>= 7;
                if (unsigned != 0)
                {
                    temp |= 128;
                }

                await WriteUnsignedByteAsync(temp);
            }
            while (unsigned != 0);
        }

        public void WritePacket(Packet packet)
        {
            WriteVarInt(packet.CalculateLength());
            WriteVarInt(packet.Id);
            WriteUnsignedBytes(packet.Data);
            Stream.Flush();
        }

        public async Task WritePacketAsync(Packet packet)
        {
            await WriteVarIntAsync(packet.CalculateLength());
            await WriteVarIntAsync(packet.Id);
            await WriteUnsignedBytesAsync(packet.Data);
            await Stream.FlushAsync(CancellationToken);
        }

        public void Dispose()
        {
            Stream.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}