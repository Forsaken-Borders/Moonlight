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
        private ILogger Logger { get; set; }

        internal async Task StartAsync()
        {
            Logger = Program.Logger.ForContext<ServerListener>();

            if (!IPAddress.TryParse(Program.Configuration.GetValue("server:ip", "0.0.0.0"), out IPAddress listeningIp))
            {
                Logger.Fatal("Ip Address {ipAddress} is not valid", Program.Configuration.GetValue("server:ip", "0.0.0.0"));
                Environment.Exit(1);
            }

            TcpListener tcpListener = new(listeningIp, Program.Configuration.GetValue("server:port", 25565));
            tcpListener.Start(Program.Configuration.GetValue("server:max_pending_connections", 100));
            while (true)
            {
                // TODO: Clean up these while loops
                while (!tcpListener.Pending())
                {
                    if (Program.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    await Task.Delay(10, Program.CancellationTokenSource.Token);
                }

                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync(Program.CancellationTokenSource.Token);
                while (tcpClient.Available == 0)
                {
                    if (Program.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    await Task.Delay(10, Program.CancellationTokenSource.Token);
                }

                PacketHandler packetHandler = new(tcpClient.GetStream(), Program.CancellationTokenSource.Token);
                HandshakePacket handshakePacket = new((await packetHandler.ReadNextPacketAsync()).Data);
                Logger.Verbose("Handshake Packet Received,\n\tProtocol Version: {version},\n\tServer Address: {address},\n\tPort: {port},\n\tNext State: {state}", handshakePacket.ProtocolVersion, handshakePacket.ServerAddress, handshakePacket.ServerPort, handshakePacket.NextClientState);

                switch (handshakePacket.NextClientState)
                {
                    case ClientState.Status:
                        Packet requestPacket = await packetHandler.ReadNextPacketAsync();
                        Logger.Verbose("Request Packet Received,\n\tId: {id}\n\tData: null", requestPacket.Id);

                        ResponsePacket responsePacket = new(new ServerStatus());
                        await packetHandler.WritePacketAsync(responsePacket);

                        PingPongPacket pingPacket = new((await packetHandler.ReadNextPacketAsync()).Data);
                        Packet pongPacket = new(0x01, pingPacket.Data);
                        await packetHandler.WritePacketAsync(pongPacket);
                        Logger.Verbose("Ping Packet Received,\n\tPacket Id: {id}\n\tPacket Data: {data}", pingPacket.Id, pingPacket.Payload);
                        Logger.Debug("{ipAddress} issued a server list ping.", (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "An unknown ip");
                        break;
                    case ClientState.Login:
                        Logger.Debug("{ipAddress} is attempting to login...", (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "An unknown ip");
                        if (handshakePacket.ProtocolVersion != 756)
                        {
                            await packetHandler.WritePacketAsync(new DisconnectPacket("You must be on version 1.17.1!"));
                            Logger.Verbose("{ipAddress} is using an unsupported protocol version {protocolVersion}. Disconnect Packet Sent,\n\tReason: You must be on version 1.17.1!", (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "An unknown ip", handshakePacket.ProtocolVersion);
                            await tcpClient.GetStream().DisposeAsync();
                            tcpClient.Dispose();
                            break;
                        }
                        Thread playerLoginThread = new(() => new MinecraftClient(tcpClient).Login());
                        playerLoginThread.Start();
                        break;
                    default:
                        // Cannot send disconnect packet here since the client is not in the Login state.
                        Logger.Warning("{ipAddress} sent an unknown packet, likely from an earlier or later version. Packet sent: {jsonPacket}", (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "An unknown ip", JsonSerializer.Serialize(handshakePacket, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
                        Logger.Warning("Unsure how to proceed, disconnecting them.");
                        await tcpClient.GetStream().DisposeAsync();
                        tcpClient.Dispose();
                        break;
                }
            }
        }
    }
}