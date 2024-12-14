using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Protocol.Net.HandshakeState
{
    public record LegacyPingPacket : HandshakePacket, ISpanSerializable<LegacyPingPacket>
    {
        public static new VarInt Id { get; } = 0xFE;

        // The compiler will helpfully add the constants together
        // at compile time, so I can leave them separated for readability
        public static int CalculateSize(LegacyPingPacket packet) =>
            1 + // Packet ID
            1 + // Server Ping Payload
            1 + // Plugin Message
            2 + // Length of "MC|PingHost"
            Encoding.BigEndianUnicode.GetByteCount("MC|PingHost") + // "MC|PingHost"
            2 + // Length of the data
            1 + // Protocol version
            2 + // Length of the hostname
            Encoding.BigEndianUnicode.GetByteCount("localhost") + // Hostname
            4; // Port

        public static int Serialize(LegacyPingPacket packet, Span<byte> target)
        {
            target[0] = 0xFE;
            target[1] = 0x01;
            target[2] = 0xFA;

            // Write the length of the "MC|PingHost" string as a short
            target[3] = 0x00;
            target[4] = 0x0b;

            // Write the "MC|PingHost" string
            int position = Encoding.BigEndianUnicode.GetBytes("MC|PingHost", target[5..]) + 5;

            // Calculate the length of the data
            int dataLength = 1 + 2 + (packet.ServerAddress.Length * 2) + 2 + 4;

            // Write the length of the data as a short
            BinaryPrimitives.WriteInt16BigEndian(target[position..], (short)dataLength);
            position += 2;

            // Write the protocol version
            target[position] = (byte)-packet.ProtocolVersion;
            position++;

            // Write the length of the hostname as a short
            BinaryPrimitives.WriteInt16BigEndian(target[position..], (short)packet.ServerAddress.Length);
            position += 2;

            // Write the hostname
            position = Encoding.BigEndianUnicode.GetBytes(packet.ServerAddress, target[position..]) + position;

            // Write the port
            BinaryPrimitives.WriteInt32BigEndian(target[position..], packet.ServerPort);
            position += 4;

            // Return the position
            return position;
        }

        public static bool TryDeserialize(ref SequenceReader<byte> reader, [NotNullWhen(true)] out LegacyPingPacket? result)
        {
            if (TryDeserialize(ref reader, out HandshakePacket? packet))
            {
                result = (LegacyPingPacket)packet;
                return true;
            }

            result = null;
            return false;
        }

        public static new bool TryDeserialize(ref SequenceReader<byte> reader, [NotNullWhen(true)] out HandshakePacket? result)
        {
            // Skip past the following:
            // - 0xFE: Packet ID
            // - 0x01: Server Ping Payload
            // - 0xFA: Plugin Message
            reader.Advance(3);
            if (!reader.TryReadBigEndian(out short mcPingHostStringLength))
            {
                result = null;
                return false;
            }

            // This should always be "MC|PingHost"
            reader.Advance(mcPingHostStringLength * 2);

            // The remaining length
            if (!reader.TryReadBigEndian(out short dataLength) || reader.Remaining < dataLength)
            {
                result = null;
                return false;
            }

            int protocolVersion = reader.UnreadSpan[0];
            reader.Advance(1);

            // Hostname length
            if (!reader.TryReadBigEndian(out short hostnameLength) || reader.Remaining < hostnameLength * 2)
            {
                result = null;
                return false;
            }

            // Hostname
            if (!reader.TryReadExact(hostnameLength * 2, out ReadOnlySequence<byte> hostnameSequence))
            {
                result = null;
                return false;
            }

            // Port
            if (!reader.TryReadBigEndian(out int port))
            {
                result = null;
                return false;
            }

            result = new LegacyPingPacket()
            {
                ServerAddress = Encoding.BigEndianUnicode.GetString(hostnameSequence),
                ServerPort = (ushort)port,
                // Make the protocol version negative to indicate that this is a legacy ping packet
                // We could not do this and force the user to type check, however that could lead
                // to potential bugs if the user forgets. Since the pre-netty legacy ping packet
                // never sends any negative protocol versions anyways, this should be safe.
                ProtocolVersion = -protocolVersion,
                NextState = 1
            };

            return true;
        }

        public static new LegacyPingPacket Deserialize(ref SequenceReader<byte> reader)
        {
            // Skip past the following:
            // - 0xFE: Packet ID
            // - 0x01: Server Ping Payload
            // - 0xFA: Plugin Message
            reader.Advance(3);

            if (!reader.TryReadBigEndian(out short mcPingHostStringLength))
            {
                throw new InvalidOperationException("Unable to read MC|PingHost string length.");
            }

            // This should always be "MC|PingHost"
            reader.Advance(mcPingHostStringLength * 2);

            // The remaining length
            if (!reader.TryReadBigEndian(out short dataLength) || reader.Remaining < dataLength)
            {
                throw new InvalidOperationException("Unable to read data length.");
            }

            int protocolVersion = reader.UnreadSpan[0];
            reader.Advance(1);

            // Hostname length
            if (!reader.TryReadBigEndian(out short hostnameLength) || reader.Remaining < hostnameLength * 2)
            {
                throw new InvalidOperationException("Unable to read hostname length.");
            }

            // Hostname
            if (!reader.TryReadExact(hostnameLength * 2, out ReadOnlySequence<byte> hostnameSequence))
            {
                throw new InvalidOperationException("Unable to read hostname.");
            }

            // Port
            return !reader.TryReadBigEndian(out int port) ? throw new InvalidOperationException("Unable to read port.") : new LegacyPingPacket()
            {
                ServerAddress = Encoding.BigEndianUnicode.GetString(hostnameSequence),
                ServerPort = (ushort)port,
                // Make the protocol version negative to indicate that this is a legacy ping packet
                // We could not do this and force the user to type check, however that could lead
                // to potential bugs if the user forgets. Since the pre-netty legacy ping packet
                // never sends any negative protocol versions anyways, this should be safe.
                ProtocolVersion = -protocolVersion,
                NextState = 1
            };
        }
    }
}
