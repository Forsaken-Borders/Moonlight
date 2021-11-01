using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moonlight.Network.Packets;
using Serilog;

namespace Moonlight.Network
{
    public class ServerListener
    {
        private ILogger Logger { get; set; }

        internal async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger = Program.Logger.ForContext<ServerListener>();

            if (!IPAddress.TryParse(Program.Configuration.GetValue("server:ip", "0.0.0.0"), out IPAddress listeningIp))
            {
                Logger.Fatal("Ip Address {ipAddress} is not valid", Program.Configuration.GetValue("server:ip", "0.0.0.0"));
                Environment.Exit(1);
            }

            TcpListener tcpListener = new(listeningIp, Program.Configuration.GetValue("server:port", 25565));
            tcpListener.Start(Program.Configuration.GetValue("server:max_pending_connections", 1000));
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!tcpListener.Pending())
                {
                    await Task.Delay(10, cancellationToken);
                }

                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                while (tcpClient.Available == 0)
                {
                    await Task.Delay(10, cancellationToken);
                }

                PacketHandler packetHandler = new(tcpClient.GetStream(), cancellationToken);
                HandshakePacket handshakePacket = await packetHandler.ReadNextPacketAsync<HandshakePacket>();
                Logger.Information("Handshake Packet Received,\n\tProtocol Version: {version},\n\tServer Address: {address},\n\tPort: {port},\n\tNext State: {state}", handshakePacket.ProtocolVersion, handshakePacket.ServerAddress, handshakePacket.ServerAddress, handshakePacket.NextClientState);

                Packet requestPacket = await packetHandler.ReadNextPacketAsync<Packet>();
                Logger.Information("Request Packet Received,\n\tId: {id}\n\tData: null", requestPacket.Id);

                // TODO: Move this to a ResponsePacket class
                Packet responsePacket = new(0x00, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
                {
                    version = new
                    {
                        name = Program.Configuration.GetValue("server:name", "Moonlight 1.17.1"),
                        protocol = 756
                    },
                    players = new
                    {
                        max = 100,
                        online = 0,
                        description = new
                        {
                            text = Program.Configuration.GetValue("server:description", "Moonlight")
                        },
                    }
                }, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })));
                await packetHandler.WritePacketAsync(responsePacket);

                PingPongPacket pingPacket = await packetHandler.ReadNextPacketAsync<PingPongPacket>();
                Packet pongPacket = new(0x01, pingPacket.Data);
                await packetHandler.WritePacketAsync(pongPacket);
                Logger.Information("Ping Packet Received,\n\tPacket Id: {id}\n\tPacket Data: {data}", pingPacket.Id, Encoding.UTF8.GetString(pingPacket.Data));
            }
        }
    }
}