using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Moonlight.Api.Networking.Packets.HandshakeState
{
    public class HandshakePacket : Packet
    {
        public override int Id { get; set; } = 0x01;

        /// <summary>
        /// The protocol version that the client is connecting from.
        /// <br/>If the packet came in a format from pre-Netty rewrite (I.E, pre-1.7, specifically before snapshot <c>13w41a</c>) the number will be it's negative equivalent.
        /// <br/>Between Beta 1.8 and Beta 1.3: <c>-1</c>
        /// <br/>Between 1.4 and 1.5: <c>-2</c>
        /// <br/>Between 1.5 and 1.6 (specifically snapshot <c>13w39b</c>): It's negative equivalent of what it provides.
        /// </summary>
        public int ProtocolVersion { get; private set; } = -1;
        public IPAddress ServerAddress { get; private set; } = IPAddress.Any;
        public ushort ServerPort { get; private set; } = 25565;
        public ClientState NextClientState { get; private set; } = ClientState.Status;

        public HandshakePacket() { }

        public HandshakePacket(int id = 0, params byte[] data)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            ArgumentNullException.ThrowIfNull(data, nameof(data));

            Id = id;
            Data = data;
            UpdateProperties();
        }

        public HandshakePacket(int protocolVersion, IPAddress serverAddress, ushort serverPort, ClientState nextClientState)
        {
            ArgumentNullException.ThrowIfNull(protocolVersion);
            ArgumentNullException.ThrowIfNull(serverAddress);
            ArgumentNullException.ThrowIfNull(serverPort);
            ArgumentNullException.ThrowIfNull(nextClientState);

            ProtocolVersion = protocolVersion;
            ServerAddress = serverAddress;
            ServerPort = serverPort;
            NextClientState = nextClientState;
            UpdateData();
        }

        public override void UpdateData()
        {
            using PacketHandler packetHandler = new(new MemoryStream());
            packetHandler.WriteVarInt(CalculatePacketLength());
            packetHandler.WriteVarInt(Id);
            packetHandler.WriteVarInt(ProtocolVersion);
            packetHandler.WriteString(ServerAddress.ToString());
            packetHandler.WriteUnsignedShort(ServerPort);
            packetHandler.WriteVarInt((int)NextClientState);
            packetHandler.Stream.Position = 0;
            Data = packetHandler.ReadPacket().Data;
        }

        public override void UpdateProperties()
        {
            if (Data == null || Data.Length == 0)
            {
                return;
            }

            using PacketHandler packetHandler = new(new MemoryStream(Data));
            ProtocolVersion = packetHandler.ReadVarInt();
            string serverAddress = packetHandler.ReadString();
            if (serverAddress.ToLowerInvariant() == "localhost")
            {
                serverAddress = "127.0.0.1";
            }
            ServerAddress = IPAddress.TryParse(serverAddress, out IPAddress? ipAddress) ? ipAddress : throw new ArgumentException("Failed to parse server address; invalid data provided.");
            ServerPort = packetHandler.ReadUnsignedShort();
            int nextClientState = packetHandler.ReadVarInt();
            NextClientState = Enum.IsDefined(typeof(ClientState), nextClientState) ? (ClientState)nextClientState : throw new ArgumentException("Failed to correctly cast to a ClientStatus; invalid data provided.");
        }

        /// <summary>
        /// Reads the first (and subsequently second and third bytes, if required) of the incoming server packet. Supports all Minecraft versions.
        /// </summary>
        /// <param name="packetHandler">A PacketHandler belonging to the incoming connection.</param>
        /// <returns>A HandshakePacket. If the ProtocolVersion is in the negatives, it's a connection from an outdated client belonging to the Pre-Netty Rewrite.</returns>
        public static HandshakePacket Read(PacketHandler packetHandler)
        {
            byte packetLengthOrId = packetHandler.ReadUnsignedByte();
            switch (packetLengthOrId)
            {
                case 0xFE: // Beta 1.8-1.6 server status packet
                    int packetLength;
                    if (packetHandler.Stream.BaseStream is NetworkStream networkStream)
                    {
                        // We are specifically checking if it's a NetworkStream since NetworkStream#ReadByte() will block until either a byte is sent or until the connection is closed.
                        packetLength = networkStream.DataAvailable ? networkStream.ReadByte() : -1;
                    }
                    else
                    {
                        packetLength = packetHandler.ReadUnsignedByte();
                    }

                    if (packetLength == -1) // Beta 1.8 will only send a 0xFE byte as a server ping.
                    {
                        return new HandshakePacket()
                        {
                            Data = new byte[] { 0xFE },
                            ProtocolVersion = -1,
                            NextClientState = ClientState.Status
                        };
                    }
                    else
                    {
                        int pluginPacketIdentifier;
                        if (packetHandler.Stream.BaseStream is NetworkStream networkStream2)
                        {
                            // We are specifically checking if it's a NetworkStream since NetworkStream#ReadByte() will block until either a byte is sent or until the connection is closed.
                            pluginPacketIdentifier = networkStream2.DataAvailable ? networkStream2.ReadByte() : -1;
                        }
                        else
                        {
                            pluginPacketIdentifier = packetHandler.ReadUnsignedByte();
                        }

                        if (pluginPacketIdentifier == -1) // 1.4 to 1.5 will send [0xFE, 0x01], where 0x01 is the packet length.
                        {
                            return new HandshakePacket()
                            {
                                Data = new byte[] { 0xFE, 0x01 },
                                ProtocolVersion = -2,
                                NextClientState = ClientState.Status
                            };
                        }
                        else // 1.6 to pre-1.7. By this point, we have read three bytes: [0xFE, 0x01, 0xFA]
                        {
                            // We are not creating a new HandshakePacket instance and using UpdateData() due to encoding related things
                            short stringLength = packetHandler.ReadShort();
                            string unknownString = Encoding.BigEndianUnicode.GetString(packetHandler.ReadByteArray(stringLength * 2)); // Multiplying the length by two due to wacky encoding
                            short dataLength = packetHandler.ReadShort();
                            int protocolVersion = packetHandler.ReadByte();
                            short hostnameLength = packetHandler.ReadShort();
                            string hostname = Encoding.BigEndianUnicode.GetString(packetHandler.ReadByteArray(hostnameLength * 2));
                            int port = packetHandler.ReadInt();

                            using PacketHandler tempPacketHandler = new(new MemoryStream());
                            tempPacketHandler.WriteUnsignedByte(0xFE);
                            tempPacketHandler.WriteUnsignedByte((byte)packetLength);
                            tempPacketHandler.WriteUnsignedByte((byte)pluginPacketIdentifier);
                            tempPacketHandler.WriteShort(stringLength);
                            tempPacketHandler.WriteUnsignedByteArray(Encoding.BigEndianUnicode.GetBytes(unknownString));
                            tempPacketHandler.WriteShort(dataLength);
                            tempPacketHandler.WriteInt(protocolVersion);
                            tempPacketHandler.WriteShort(hostnameLength);
                            tempPacketHandler.WriteUnsignedByteArray(Encoding.BigEndianUnicode.GetBytes(hostname));
                            tempPacketHandler.WriteInt(port);
                            tempPacketHandler.Stream.Position = 0;
                            byte[] data = new byte[17 + Encoding.BigEndianUnicode.GetByteCount(unknownString) + Encoding.BigEndianUnicode.GetByteCount(hostname)];
                            tempPacketHandler.Stream.Read(data);

                            HandshakePacket handshakePacket = new()
                            {
                                Data = data,
                                ProtocolVersion = -protocolVersion,
                                ServerAddress = IPAddress.Parse(hostname),
                                ServerPort = (ushort)port,
                                NextClientState = ClientState.Status
                            };

                            return handshakePacket;
                        }
                    }
                case 0x02: // Pre 1.7 login packet. Only reading the first byte, but we should probably make a way to get the full packet for the plugin API later...
                    return new HandshakePacket()
                    {
                        Data = new byte[] { 0x02 },
                        ProtocolVersion = -1,
                        NextClientState = ClientState.Login
                    };
                default: // Netty Rewrite handshake packet, I.E 1.7+
                    packetLength = ReadVarInt(packetHandler, packetLengthOrId);
                    return packetHandler.ReadPacket<HandshakePacket>(packetLength);
            }
        }

        public override int CalculatePacketLength() => Id.GetVarLength() + ServerAddress.GetVarLength() + sizeof(ushort) + sizeof(int);

        private static int ReadVarInt(PacketHandler packetHandler, byte? read = null)
        {
            // We create our own ReadVarInt here since PacketHandler.ReadVarInt only supports reading from the beginning of the VarInt, not from the middle.
            // This method provides the first byte to the VarInt, at the cost of checking if it's null everytime. Huge performance loss, but it's only called during a server list ping.
            // I hate this, but I couldn't find any other solution.
            // FIXME: Free PR
            int numRead = 0;
            int result = 0;
            do
            {
                read ??= packetHandler.ReadUnsignedByte();
                int value = read.Value & 0b01111111;
                result |= value << (7 * numRead);

                numRead++;
                if (numRead > 5)
                {
                    throw new InvalidOperationException("VarInt is too big");
                }
            } while ((read & 0b10000000) != 0);

            return result;
        }
    }

    public enum ClientState
    {
        Status = 1,
        Login = 2
    }
}