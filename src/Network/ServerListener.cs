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
                    await Task.Delay(10);
                }

                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                while (tcpClient.Available == 0)
                {
                    await Task.Delay(10, cancellationToken);
                }

                PacketHandler packetHandler = new(tcpClient.GetStream(), cancellationToken);
                HandshakePacket handshakePacket = await HandshakePacket.Create((await packetHandler.ReadNextPacketAsync()).Data);
                Logger.Information("Handshake Packet Recieved,\n\tProtocol Version: {version},\n\tServer Address: {address},\n\tPort: {port},\n\tNext State: {state}", handshakePacket.ProtocolVersion, handshakePacket.ServerAddress, handshakePacket.ServerAddress, handshakePacket.NextClientState);

                Packet requestPacket = await packetHandler.ReadNextPacketAsync();
                Logger.Information("Request Packet Recieved,\n\tId: {id}\n\tData: null", requestPacket.Id);

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
                        online = 5,
                        sample = new[] {
                                new {
                                    name = "thinkofdeath",
                                    id = "4566e69f-c907-48ee-8d71-d7ba5aa00d20"
                                }
                            },
                        description = new
                        {
                            text = Program.Configuration.GetValue("server:description", "Moonlight: A C# implementation of the Minecraft Server Protocol.")
                        },
                    }
                }, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })));
                await packetHandler.WritePacketAsync(responsePacket);

                Packet pingPacket = await packetHandler.ReadNextPacketAsync();
                Packet pongPacket = new(0x01, pingPacket.Data);
                await packetHandler.WritePacketAsync(pongPacket);
                Logger.Information("Ping Packet Recieved,\n\tPacket Id: {id}\n\tPacket Data: {data}", pingPacket.Id, Encoding.UTF8.GetString(pingPacket.Data));
            }
        }
    }
}