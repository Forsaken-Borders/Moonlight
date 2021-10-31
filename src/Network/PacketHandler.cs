using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moonlight.Network.Packets;
using Serilog;

namespace Moonlight.Network
{
    public class PacketHandler
    {
        public Stream Stream { get; init; }
        public CancellationToken CancellationToken { get; init; }
        private ILogger Logger { get; init; } // TODO: Logging

        public PacketHandler(Stream stream, CancellationToken cancellationToken = new())
        {
            Stream = stream;
            Logger = Program.Logger.ForContext<PacketHandler>();
            CancellationToken = cancellationToken;
        }

        public async Task<byte> ReadUnsignedByteAsync()
        {
            byte[] totalLength = new byte[1];
            await Stream.ReadAsync(totalLength, CancellationToken);
            return totalLength[0];
        }

        public async Task<sbyte> ReadSignedByteAsync()
        {
            byte[] totalLength = new byte[1];
            await Stream.ReadAsync(totalLength, CancellationToken);
            return (sbyte)totalLength[0];
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

        public async Task<ushort> ReadUnsignedShortAsync()
        {
            byte[] buffer = new byte[2];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }

        public async Task<short> ReadShortAsync()
        {
            byte[] buffer = new byte[2];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadInt16BigEndian(buffer);
        }

        public async Task<int> ReadIntAsync()
        {
            byte[] buffer = new byte[4];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadInt32BigEndian(buffer);
        }

        public async Task<long> ReadLongAsync()
        {
            byte[] buffer = new byte[8];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadInt64BigEndian(buffer);
        }

        public async Task<ulong> ReadUnsignedLongAsync()
        {
            byte[] buffer = new byte[8];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadUInt64BigEndian(buffer);
        }

        public async Task<float> ReadFloatAsync()
        {
            byte[] buffer = new byte[4];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadSingleBigEndian(buffer);
        }

        public async Task<double> ReadDoubleAsync()
        {
            byte[] buffer = new byte[8];
            await Stream.ReadAsync(buffer, CancellationToken);
            return BinaryPrimitives.ReadDoubleBigEndian(buffer);
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

        public async Task<Packet> ReadNextPacketAsync()
        {
            int totalPacketLength = await ReadVarIntAsync();
            int packetId = await ReadVarIntAsync();
            int packetDataLength = totalPacketLength - packetId.GetVarIntLength();

            if (packetDataLength <= 0)
            {
                return new(packetId);
            }

            byte[] packetData = new byte[packetDataLength];
            await Stream.ReadAsync(packetData, default);

            return new(packetId, packetData);
        }

        public async Task WriteUnsignedByteAsync(byte value) => await Stream.WriteAsync(new[] { value }, CancellationToken);
        public async Task WriteUnsignedBytesAsync(byte[] values) => await Stream.WriteAsync(values, CancellationToken);
        public async Task WriteSignedByteAsync(sbyte value) => await WriteUnsignedByteAsync((byte)value);
        public async Task WriteSignedBytesAsync(sbyte[] values) => await WriteUnsignedBytesAsync(values.Cast<byte>().ToArray());
        public async Task WriteBooleanAsync(bool value) => await WriteUnsignedByteAsync((byte)(value ? 0x01 : 0x00));

        public async Task WriteUnsignedShortAsync(ushort value)
        {
            byte[] write = new byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16BigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public async Task WriteSignedShortAsync(short value)
        {
            byte[] write = new byte[sizeof(short)];
            BinaryPrimitives.WriteInt16BigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public async Task WriteIntAsync(int value)
        {
            byte[] write = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public async Task WriteLongAsync(long value)
        {
            byte[] write = new byte[sizeof(long)];
            BinaryPrimitives.WriteInt64BigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public async Task WriteFloatAsync(float value)
        {
            byte[] write = new byte[sizeof(float)];
            BinaryPrimitives.WriteSingleBigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public async Task WriteDoubleAsync(double value)
        {
            byte[] write = new byte[sizeof(double)];
            BinaryPrimitives.WriteDoubleBigEndian(write, value);
            await Stream.WriteAsync(write, CancellationToken);
        }

        public async Task WriteStringAsync(string value, int maxLength = short.MaxValue)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            else if (value.Length > maxLength)
            {
                throw new ArgumentException($"string ({value.Length}) exceeded maximum length ({maxLength})", nameof(value));
            }

            byte[] write = Encoding.UTF8.GetBytes(value);
            await WriteVarIntAsync(write.Length);
            await Stream.WriteAsync(write, CancellationToken);
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

        public async Task WritePacketAsync(Packet packet)
        {
            await WriteVarIntAsync(packet.CalculateLength());
            await WriteVarIntAsync(packet.Id);
            await WriteUnsignedBytesAsync(packet.Data);
            await Stream.FlushAsync(CancellationToken);
        }

        public async Task WritePacketAsync(int id, byte[] data)
        {
            await WriteVarIntAsync(id.GetVarIntLength() + (data?.Length ?? 0));
            await WriteVarIntAsync(id);
            await WriteUnsignedBytesAsync(data);
            await Stream.FlushAsync(CancellationToken);
        }
    }
}