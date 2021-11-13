using System;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
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
        private ILogger Logger { get; init; } = Program.Logger.ForContext<ServerListener>();

        internal async Task StartAsync(CancellationToken cancellationToken)
        {
            // Dns.GetHostEntry complains if 0.0.0.0 or the ipv6 equivalent is given. As such, we must check to see if it's either of those two specifically.
            // If it isn't, then try having the DNS resolve the listening address.
            if (!IPAddress.TryParse(Program.Configuration.GetValue("server:ip", "0.0.0.0"), out IPAddress listeningAddress) && listeningAddress != IPAddress.Any)
            {
                try
                {
                    listeningAddress = Dns.GetHostEntry(Program.Configuration.GetValue("server:ip", "0.0.0.0")).AddressList[0];
                }
                catch (Exception exception)
                {
                    Logger.Fatal("Ip Address {ipAddress} is not valid. Error message:\n\t{errorMessage}", Program.Configuration.GetValue("server:ip", "0.0.0.0"), exception.Message);
                    Logger.Fatal("Shutting down...");

                    // TODO: Make and use a server shutdown function, complete with events.
                    Environment.Exit(1);
                }
            }

            TcpListener tcpListener = new(listeningAddress, Program.Configuration.GetValue("server:port", 25565));
            tcpListener.Start(Program.Configuration.GetValue("server:max_pending_connections", 100));
            Logger.Information("Server listening on {address}, port {port}", Program.Configuration.GetValue("server:ip", "0.0.0.0"), Program.Configuration.GetValue("server:port", 25565));

            while (!cancellationToken.IsCancellationRequested)
            {
                while (!tcpListener.Pending())
                {
                    await Task.Delay(10, cancellationToken);
                }

                await HandleNewConnection(await tcpListener.AcceptTcpClientAsync(cancellationToken), cancellationToken);
            }
        }

        internal async Task HandleNewConnection(TcpClient tcpClient, CancellationToken cancellationToken)
        {
            Logger.Verbose("Accepted connection from {ipAddress}", (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "an unknown ip.");

            // TODO: Correctly handle a legacy server ping.
            PacketHandler packetHandler = new(tcpClient.GetStream(), cancellationToken);
            HandshakePacket handshakePacket = new((await packetHandler.ReadNextPacketAsync()).Data);
            Logger.Verbose("Handshake Packet Received,\n\tProtocol Version: {version},\n\tServer Address: {address},\n\tPort: {port},\n\tNext State: {state}", handshakePacket.ProtocolVersion, handshakePacket.ServerAddress, handshakePacket.ServerPort, handshakePacket.NextClientState);

            switch (handshakePacket.NextClientState)
            {
                case ClientState.Status:
                    Packet requestPacket = await packetHandler.ReadNextPacketAsync();
                    Logger.Verbose("Request Packet Received,\n\tId: {id}\n\tData: null", requestPacket.Id);

                    // By default, an empty ServerStatus constructor will grab the correct values from the config.
                    ResponsePacket responsePacket = new(new ServerStatus());
                    await packetHandler.WritePacketAsync(responsePacket);
                    Logger.Debug("{ipAddress} issued a server list ping.", (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "An unknown ip");

                    // Ping packet is optional, we shouldn't expect it.
                    Packet optionalPingPacket = await packetHandler.ReadNextPacketAsync();
                    if (optionalPingPacket != null)
                    {
                        PingPongPacket pingPacket = new(optionalPingPacket.Data);
                        Packet pongPacket = new(0x01, pingPacket.Data);
                        await packetHandler.WritePacketAsync(pongPacket);
                        Logger.Verbose("Ping Packet Received,\n\tPacket Id: {id}\n\tPacket Data: {data}", pingPacket.Id, pingPacket.Payload);
                    }

                    break;
                case ClientState.Login:
                    Logger.Debug("{ipAddress} is attempting to login...", (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "An unknown ip");
                    if (handshakePacket.ProtocolVersion != 756)
                    {
                        await packetHandler.WritePacketAsync(new DisconnectPacket(Program.Configuration.GetValue("server:invalid_protocol_message", "You must be on version 1.17.1!")));
                        Logger.Verbose("{ipAddress} is using an unsupported protocol version: {protocolVersion}. Disconnect Packet Sent,\n\tReason: {invalidProtocolMessage}", (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "An unknown ip", handshakePacket.ProtocolVersion, Program.Configuration.GetValue("server:invalid_protocol_message", "You must be on version 1.17.1!"));
                        packetHandler.Dispose();
                        tcpClient.Dispose();
                        break;
                    }

                    new MinecraftClient(tcpClient, cancellationToken).Login();
                    break;
                default:
                    // Cannot send disconnect packet here since the client is not in the Login state.
                    Logger.Warning("{ipAddress} sent an unknown packet, likely from an earlier or later version. Packet sent: {jsonPacket}", (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "An unknown ip", JsonSerializer.Serialize(handshakePacket, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
                    Logger.Warning("Unsure how to proceed, disconnecting them.");
                    packetHandler.Dispose();
                    tcpClient.Dispose();
                    break;
            }
        }
    }
}