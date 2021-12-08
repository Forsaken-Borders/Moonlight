using System.Buffers.Binary;
using System.Text;
using Moonlight.Api.Networking.Packets;

namespace Moonlight.Api.Networking
{
    public class PacketHandler : IDisposable, IAsyncDisposable
    {
        public virtual MinecraftStream Stream { get; private set; }
        public CancellationToken CancellationToken { get; set; }

        public PacketHandler(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));
            ArgumentNullException.ThrowIfNull(cancellationToken, nameof(cancellationToken));

            Stream = new(stream);
            CancellationToken = cancellationToken;
        }

        #region ReadMethods
        public virtual byte ReadUnsignedByte() => (byte)Stream.ReadByte();
        public virtual sbyte ReadByte() => (sbyte)Stream.ReadByte();
        public virtual bool ReadBool() => Stream.ReadByte() switch
        {
            0x00 => false,
            0x01 => true,
            _ => throw new ArgumentOutOfRangeException("Byte returned by stream is out of range (0x00 or 0x01)", nameof(Stream))
        };

        public virtual ushort ReadUnsignedShort()
        {
            byte[] buffer = new byte[2];
            Stream.Read(buffer);
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }

        public virtual short ReadShort()
        {
            byte[] buffer = new byte[2];
            Stream.Read(buffer);
            return BinaryPrimitives.ReadInt16BigEndian(buffer);
        }

        public virtual int ReadInt()
        {
            byte[] buffer = new byte[4];
            Stream.Read(buffer);
            return BinaryPrimitives.ReadInt32BigEndian(buffer);
        }

        public virtual ulong ReadUnsignedLong()
        {
            byte[] buffer = new byte[8];
            Stream.Read(buffer);
            return BinaryPrimitives.ReadUInt64BigEndian(buffer);
        }

        public virtual long ReadLong()
        {
            byte[] buffer = new byte[8];
            Stream.Read(buffer);
            return BinaryPrimitives.ReadInt64BigEndian(buffer);
        }

        public virtual float ReadFloat()
        {
            byte[] buffer = new byte[4];
            Stream.Read(buffer);
            return BinaryPrimitives.ReadSingleBigEndian(buffer);
        }

        public virtual double ReadDouble()
        {
            byte[] buffer = new byte[8];
            Stream.Read(buffer);
            return BinaryPrimitives.ReadDoubleBigEndian(buffer);
        }

        public virtual string ReadString(int maxLength = short.MaxValue)
        {
            int length = ReadVarInt();
            byte[] buffer = new byte[length];
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            Stream.Read(buffer);

            string value = Encoding.UTF8.GetString(buffer);
            return maxLength > 0 && value.Length > maxLength
                ? throw new InvalidOperationException($"String ({value.Length}) exceeded maximum length ({maxLength})")
                : value;
        }

        public virtual int ReadVarInt()
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

        public virtual long ReadVarLong()
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

        public virtual byte[] ReadByteArray(int length = 0)
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

        public Packet ReadPacket(int packetLength = -1)
        {
            if (Stream.CompressionEnabled)
            {
                PacketHandler uncompressedPacketHandler = new(Stream.BaseStream, CancellationToken);
                packetLength = uncompressedPacketHandler.ReadVarInt();

                if (packetLength == 0)
                {
                    return new Packet();
                }

                int dataLength = uncompressedPacketHandler.ReadVarInt();
                packetLength = -dataLength.GetVarLength();
                return packetLength <= 0 ? new Packet() : ReadUncompressedPacket(dataLength);
            }
            else
            {
                return ReadUncompressedPacket(packetLength);
            }
        }

        public T ReadPacket<T>(int packetLength = -1) where T : Packet, new()
        {
            if (Stream.CompressionEnabled)
            {
                PacketHandler uncompressedPacketHandler = new(Stream.BaseStream, CancellationToken);
                packetLength = uncompressedPacketHandler.ReadVarInt();

                if (packetLength == 0)
                {
                    return new T();
                }

                int dataLength = uncompressedPacketHandler.ReadVarInt();
                packetLength = -dataLength.GetVarLength();
                return packetLength <= 0 ? new T() : ReadUncompressedPacket<T>(dataLength);
            }
            else
            {
                return ReadUncompressedPacket<T>(packetLength);
            }
        }

        private Packet ReadUncompressedPacket(int length = -1)
        {
            if (length == -1)
            {
                length = ReadVarInt();
            }

            int id = ReadVarInt();
            int dataLength = length - id.GetVarLength();

            if (dataLength <= 0)
            {
                return new Packet()
                {
                    Id = id
                };
            }

            byte[] packetData = new byte[dataLength];
            Stream.Read(packetData);

            Packet packet = new()
            {
                Id = id,
                Data = packetData
            };
            return packet;
        }

        private T ReadUncompressedPacket<T>(int packetLength = -1) where T : Packet, new()
        {
            if (packetLength == -1)
            {
                packetLength = ReadVarInt();
            }

            int id = ReadVarInt();
            int dataLength = packetLength - id.GetVarLength();

            if (dataLength <= 0)
            {
                return new T()
                {
                    Id = id
                };
            }

            byte[] packetData = new byte[dataLength];
            Stream.Read(packetData);

            T packet = new()
            {
                Id = id,
                Data = packetData
            };
            packet.UpdateProperties();
            return packet;
        }
        #endregion ReadMethods

        #region ReadAsyncMethods
        public virtual async Task<ushort> ReadUnsignedShortAsync()
        {
            byte[] buffer = new byte[2];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }

        public virtual async Task<short> ReadShortAsync()
        {
            byte[] buffer = new byte[2];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadInt16BigEndian(buffer);
        }

        public virtual async Task<int> ReadIntAsync()
        {
            byte[] buffer = new byte[4];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadInt32BigEndian(buffer);
        }

        public virtual async Task<ulong> ReadUnsignedLongAsync()
        {
            byte[] buffer = new byte[8];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadUInt64BigEndian(buffer);
        }

        public virtual async Task<long> ReadLongAsync()
        {
            byte[] buffer = new byte[8];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadInt64BigEndian(buffer);
        }

        public virtual async Task<float> ReadFloatAsync()
        {
            byte[] buffer = new byte[4];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadSingleBigEndian(buffer);
        }

        public virtual async Task<double> ReadDoubleAsync()
        {
            byte[] buffer = new byte[8];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadDoubleBigEndian(buffer);
        }

        public virtual async Task<string> ReadStringAsync(int maxLength = short.MaxValue)
        {
            int length = ReadVarInt();
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

        public virtual async Task<byte[]> ReadByteArrayAsync(int length = -1)
        {
            if (length == -1)
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
                n -= await Stream.ReadAsync(result.AsMemory(length - n, n));
                if (n == 0)
                {
                    break;
                }
            }
            return result;
        }

        public virtual async Task<Packet> ReadPacketAsync(int packetLength = -1)
        {
            if (Stream.CompressionEnabled)
            {
                PacketHandler uncompressedPacketHandler = new(Stream.BaseStream, CancellationToken);
                packetLength = uncompressedPacketHandler.ReadVarInt();

                if (packetLength == 0)
                {
                    return new Packet();
                }

                int dataLength = uncompressedPacketHandler.ReadVarInt();
                packetLength = -dataLength.GetVarLength();
                return packetLength <= 0 ? new Packet() : await ReadUncompressedPacketAsync(dataLength);
            }
            else
            {
                return await ReadUncompressedPacketAsync(packetLength);
            }
        }

        public virtual async Task<T> ReadPacketAsync<T>(int packetLength = -1) where T : Packet, new()
        {
            if (Stream.CompressionEnabled)
            {
                PacketHandler uncompressedPacketHandler = new(Stream.BaseStream, CancellationToken);
                packetLength = uncompressedPacketHandler.ReadVarInt();

                if (packetLength == 0)
                {
                    return new T();
                }

                int dataLength = uncompressedPacketHandler.ReadVarInt();
                packetLength = -dataLength.GetVarLength();
                return packetLength <= 0 ? new T() : await ReadUncompressedPacketAsync<T>(dataLength);
            }
            else
            {
                return await ReadUncompressedPacketAsync<T>(packetLength);
            }
        }

        private async Task<Packet> ReadUncompressedPacketAsync(int length = -1)
        {
            if (length == -1)
            {
                length = ReadVarInt();
            }
            int packetId = ReadVarInt();
            int packetDataLength = length - packetId.GetVarLength();

            if (packetDataLength <= 0)
            {
                return new Packet()
                {
                    Id = packetId
                };
            }

            byte[] packetData = new byte[packetDataLength];
            await Stream.ReadAsync(packetData, CancellationToken);

            Packet packet = new()
            {
                Id = packetId,
                Data = packetData
            };
            return packet;
        }

        private async Task<T> ReadUncompressedPacketAsync<T>(int length = -1) where T : Packet, new()
        {
            if (length == -1)
            {
                length = ReadVarInt();
            }
            int packetId = ReadVarInt();
            int packetDataLength = length - packetId.GetVarLength();

            if (packetDataLength <= 0)
            {
                return new T()
                {
                    Id = packetId
                };
            }

            byte[] packetData = new byte[packetDataLength];
            await Stream.ReadAsync(packetData, CancellationToken);

            T packet = new()
            {
                Id = packetId,
                Data = packetData
            };
            packet.UpdateProperties();
            return packet;
        }
        #endregion ReadAsyncMethods

        #region WriteMethods
        public virtual void WriteUnsignedByte(byte value) => Stream.WriteByte(value);
        public virtual void WriteByte(sbyte value) => Stream.WriteByte((byte)value);
        public virtual void WriteBool(bool value) => Stream.WriteByte((byte)(value ? 0x01 : 0x00));
        public virtual void WriteUnsignedByteArray(byte[] values) => Stream.Write(values);

        public virtual void WriteUnsignedShort(ushort value)
        {
            byte[] write = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(write, value);
            Stream.Write(write);
        }

        public virtual void WriteShort(short value)
        {
            byte[] write = new byte[2];
            BinaryPrimitives.WriteInt16BigEndian(write, value);
            Stream.Write(write);
        }

        public virtual void WriteUnsignedInt(uint value)
        {
            byte[] write = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(write, value);
            Stream.Write(write);
        }

        public virtual void WriteInt(int value)
        {
            byte[] write = new byte[4];
            BinaryPrimitives.WriteInt32BigEndian(write, value);
            Stream.Write(write);
        }

        public virtual void WriteUnsignedlong(ulong value)
        {
            byte[] write = new byte[8];
            BinaryPrimitives.WriteUInt64BigEndian(write, value);
            Stream.Write(write);
        }

        public virtual void WriteLong(long value)
        {
            byte[] write = new byte[8];
            BinaryPrimitives.WriteInt64BigEndian(write, value);
            Stream.Write(write);
        }

        public virtual void WriteFloat(float value)
        {
            byte[] write = new byte[4];
            BinaryPrimitives.WriteSingleBigEndian(write, value);
            Stream.Write(write);
        }

        public virtual void WriteDouble(double value)
        {
            byte[] write = new byte[8];
            BinaryPrimitives.WriteDoubleBigEndian(write, value);
            Stream.Write(write);
        }

        public virtual void WriteString(string value, int maxLength = short.MaxValue)
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

        public virtual void WriteVarInt(int value)
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

        public virtual void WriteVarLong(long value)
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

        public virtual void WritePacket(Packet value)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(value.Data);

            WriteVarInt(value.CalculatePacketLength());
            WriteVarInt(value.Id);
            WriteUnsignedByteArray(value.Data);
        }
        #endregion WriteMethods

        #region WriteAsyncMethods
        public virtual async Task WriteUnsignedByteArrayAsync(byte[] values) => await Stream.WriteAsync(values, CancellationToken);

        public virtual async Task WriteUnsignedShortAsync(ushort value)
        {
            byte[] write = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public virtual async Task WriteShortAsync(short value)
        {
            byte[] write = new byte[2];
            BinaryPrimitives.WriteInt16BigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public virtual async Task WriteUnsignedIntAsync(uint value)
        {
            byte[] write = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public virtual async Task WriteIntAsync(int value)
        {
            byte[] write = new byte[4];
            BinaryPrimitives.WriteInt32BigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public virtual async Task WriteUnsignedlongAsync(ulong value)
        {
            byte[] write = new byte[8];
            BinaryPrimitives.WriteUInt64BigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }


        public virtual async Task WriteLongAsync(long value)
        {
            byte[] write = new byte[8];
            BinaryPrimitives.WriteInt64BigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public virtual async Task WriteFloatAsync(float value)
        {
            byte[] write = new byte[4];
            BinaryPrimitives.WriteSingleBigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public virtual async Task WriteDoubleAsync(double value)
        {
            byte[] write = new byte[8];
            BinaryPrimitives.WriteDoubleBigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public virtual async Task WriteStringAsync(string value, int maxLength = short.MaxValue)
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

        public virtual async Task WriteVarIntAsync(int value)
        {
            uint unsigned = (uint)value;
            IEnumerable<byte> data = new byte[5];

            do
            {
                byte temp = (byte)(unsigned & 127);
                unsigned >>= 7;
                if (unsigned != 0)
                {
                    temp |= 128;
                }

                data = data.Append(temp);
            }
            while (unsigned != 0);

            await WriteUnsignedByteArrayAsync(data.ToArray());
        }

        public virtual async Task WriteVarLongAsync(long value)
        {
            ulong unsigned = (ulong)value;
            IEnumerable<byte> data = new byte[5];

            do
            {
                byte temp = (byte)(unsigned & 127);
                unsigned >>= 7;
                if (unsigned != 0)
                {
                    temp |= 128;
                }

                data = data.Append(temp);
            }
            while (unsigned != 0);

            await WriteUnsignedByteArrayAsync(data.ToArray());
        }

        public virtual async Task WritePacketAsync(Packet value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            ArgumentNullException.ThrowIfNull(value.Data, nameof(value.Data));

            await WriteVarIntAsync(value.CalculatePacketLength());
            await WriteVarIntAsync(value.Id);
            await WriteUnsignedByteArrayAsync(value.Data);
        }
        #endregion WriteAsyncMethods

        public virtual void Dispose()
        {
            Stream.Dispose();
            GC.SuppressFinalize(this);
        }

        public virtual async ValueTask DisposeAsync()
        {
            await Stream.DisposeAsync();
            GC.SuppressFinalize(this);
        }

        public override bool Equals(object? obj) => obj is PacketHandler handler && EqualityComparer<MinecraftStream>.Default.Equals(Stream, handler.Stream) && EqualityComparer<CancellationToken>.Default.Equals(CancellationToken, handler.CancellationToken);
        public override int GetHashCode() => HashCode.Combine(Stream, CancellationToken);
    }
}