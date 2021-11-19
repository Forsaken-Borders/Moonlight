using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moonlight.Network.Packets;
using Moonlight.Types;
using Serilog;

namespace Moonlight.Network
{
    public class ServerListener
    {
        private readonly ILogger Logger = Server.Logger.ForContext<ServerListener>();

        internal async Task StartAsync(CancellationToken cancellationToken)
        {
            // Dns.GetHostEntry complains if 0.0.0.0 or the ipv6 equivalent is given. As such, we must check to see if it's either of those two specifically.
            // If it isn't, then try having the DNS resolve the listening address.
            if (!IPAddress.TryParse(Server.Configuration.GetValue("server:ip", "0.0.0.0"), out IPAddress listeningAddress) && listeningAddress != IPAddress.Any)
            {
                try
                {
                    listeningAddress = Dns.GetHostEntry(Server.Configuration.GetValue("server:ip", "0.0.0.0")).AddressList[0];
                }
                catch (Exception exception)
                {
                    Logger.Fatal("Ip Address {ipAddress} is not valid. Error message:\n\t{errorMessage}", Server.Configuration.GetValue("server:ip", "0.0.0.0"), exception.Message);
                    Logger.Fatal("Shutting down...");

                    // TODO: Make and use a server shutdown function, complete with events.
                    Environment.Exit(1);
                }
            }

            TcpListener tcpListener = new(listeningAddress, Server.Configuration.GetValue("server:port", 25565));
            tcpListener.Start(Server.Configuration.GetValue("server:max_pending_connections", 100));
            Logger.Information("Server listening on {address}, port {port}", Server.Configuration.GetValue("server:ip", "0.0.0.0"), Server.Configuration.GetValue("server:port", 25565));

            while (!cancellationToken.IsCancellationRequested)
            {
                while (!tcpListener.Pending())
                {
                    await Task.Delay(10, cancellationToken);
                }
                ThreadPool.QueueUserWorkItem(async (cb) => await HandleNewConnection(await tcpListener.AcceptTcpClientAsync(), cancellationToken), cancellationToken);
            }
        }

        internal async Task HandleNewConnection(TcpClient tcpClient, CancellationToken cancellationToken)
        {
            Logger.Verbose("Accepted connection from {ipAddress}", (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "an unknown ip.");
            PacketHandler packetHandler = new(tcpClient.GetStream(), cancellationToken);
            try
            {
                HandshakePacket handshakePacket = HandshakePacket.Read(packetHandler);
                Logger.Verbose("Handshake Packet Received,\n\tProtocol Version: {version},\n\tServer Address: {address},\n\tPort: {port},\n\tNext State: {state}", handshakePacket.ProtocolVersion, handshakePacket.ServerAddress, handshakePacket.ServerPort, handshakePacket.NextClientState);

                switch (handshakePacket.NextClientState)
                {
                    case ClientState.Status:
                        if (handshakePacket.ProtocolVersion < 0) // Pre-Netty Rewrite server ping packet. If the client's version is before 1.7, the ProtocolVersion should be in the negatives.
                        {
                            ServerStatus serverStatus = new();
                            string serverStatusDescription = serverStatus.Description.ToString(handshakePacket.ProtocolVersion == -1 ? ColorParseMode.None : ColorParseMode.Translate);

                            // In Beta 1.8, the size for a server status packet cannot exceed 64 bytes, packet id and packet length excluded
                            if (handshakePacket.ProtocolVersion == -1)
                            {
                                // FIXME: I have a feeling this can be optimized, allowing more MOTD chars, but I don't know how.
                                int onlinePlayerByteLength = Encoding.BigEndianUnicode.GetByteCount(new char[] { (char)serverStatus.Players.OnlinePlayerCount });
                                int maxPlayerByteLength = Encoding.BigEndianUnicode.GetByteCount(new char[] { (char)serverStatus.Players.MaxPlayerCount });
                                int serverMotdLength = 64 - (4 + onlinePlayerByteLength + maxPlayerByteLength);
                                if (serverStatusDescription.Length > serverMotdLength)
                                {
                                    serverStatusDescription = string.Join("", serverStatusDescription.Take(serverMotdLength - 5)) + "[...]";
                                }
                            }

                            byte[] data = Encoding.BigEndianUnicode.GetBytes((handshakePacket.ProtocolVersion is -1)
                                ? $"{serverStatusDescription}§{serverStatus.Players.OnlinePlayerCount}§{serverStatus.Players.MaxPlayerCount}"// Beta 1.8 - Beta 1.3
                                : $"§1\0127\01.17.1\0{serverStatusDescription}\0{serverStatus.Players.OnlinePlayerCount}\0{serverStatus.Players.MaxPlayerCount}"// 1.4+ protocol
                            );

                            packetHandler.WriteUnsignedByte(0xFF); // Packet Id
                            packetHandler.WriteShort((short)(data.Length / 2)); // Packet length, divide by two for unknown encoding reasons.
                            packetHandler.WriteUnsignedBytes(data);
                        }
                        else
                        {
                            // Awaiting this packet here since it's the smallest packet in the method. We still want the method to run async, but async methods should not be used due to latency.
                            Packet requestPacket = await packetHandler.ReadNextPacketAsync();
                            Logger.Verbose("Request Packet Received,\n\tId: {id}\n\tData: null", requestPacket.Id);

                            // By default, an empty ServerStatus constructor will grab the correct values from the config.
                            ResponsePacket responsePacket = new(new ServerStatus());
                            if (handshakePacket.ProtocolVersion < 713) // If the client's version is before 1.16, send stylize everything in the text.
                            {
                                responsePacket.Payload.Description.Text = responsePacket.Payload.Description.ToString(ColorParseMode.Translate);
                                responsePacket.Payload.Description.Extra = null;
                                responsePacket.UpdateData();
                            }
                            packetHandler.WritePacket(responsePacket);
                            Logger.Debug("{ipAddress} issued a server list ping.", (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "An unknown ip");

                            // Ping packet is optional, we shouldn't expect it.
                            if (tcpClient.Connected)
                            {
                                PingPongPacket pingPacket = new(packetHandler.ReadNextPacket().Data);
                                Packet pongPacket = new(0x01, pingPacket.Data);
                                packetHandler.WritePacket(pongPacket);
                                Logger.Verbose("Ping Packet Received,\n\tPacket Id: {id}\n\tPacket Data: {data}", pingPacket.Id, pingPacket.Payload);
                            }
                        }

                        packetHandler.Dispose();
                        tcpClient.Dispose();
                        break;
                    case ClientState.Login:
                        Logger.Debug("{ipAddress} is attempting to login...", (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "An unknown ip");
                        if (handshakePacket.ProtocolVersion == -1)
                        {
                            byte[] data = Encoding.BigEndianUnicode.GetBytes(Server.Configuration.GetValue("server:invalid_protocol_message", "You must be on version 1.17.1!"));
                            packetHandler.WriteUnsignedByte(0xFF); // Packet Id
                            packetHandler.WriteShort((short)(data.Length / 2)); // Packet length
                            packetHandler.WriteUnsignedBytes(data);
                            packetHandler.Dispose();
                            tcpClient.Dispose();
                        }
                        else if (handshakePacket.ProtocolVersion == ServerVersion.CurrentProtocol)
                        {
                            new MinecraftClient(tcpClient, cancellationToken).Login();
                        }
                        else
                        {
                            packetHandler.WritePacket(new DisconnectPacket(Server.Configuration.GetValue("server:invalid_protocol_message", "You must be on version 1.17.1!")));
                            Logger.Verbose("{ipAddress} is using an unsupported protocol version: {protocolVersion}. Disconnect Packet Sent,\n\tReason: {invalidProtocolMessage}", (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "An unknown ip", handshakePacket.ProtocolVersion, Server.Configuration.GetValue("server:invalid_protocol_message", "You must be on version 1.17.1!"));
                            packetHandler.Dispose();
                            tcpClient.Dispose();
                        }
                        break;
                    default:
                        // Cannot send disconnect packet here since the client is not in the Login state.
                        Logger.Warning("{ipAddress} sent an unknown packet, likely from an earlier or later version. Packet sent: {@jsonPacket}", (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "An unknown ip", handshakePacket);
                        Logger.Warning("Unsure how to proceed, disconnecting them.");
                        packetHandler.Dispose();
                        tcpClient.Dispose();
                        break;
                }
            }
            catch (IOException)
            {
                packetHandler.Dispose();
                tcpClient.Dispose();
                return;
            }
            catch (SocketException)
            {
                packetHandler.Dispose();
                tcpClient.Dispose();
                return;
            }
            catch (InvalidOperationException)
            {
                packetHandler.Dispose();
                tcpClient.Dispose();
                return;
            }
        }
    }
}