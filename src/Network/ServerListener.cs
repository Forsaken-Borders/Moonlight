using System;
using System.Net;
using System.Net.Sockets;
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

        internal async Task StartAsync(CancellationToken cancellationToken)
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
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    await Task.Delay(10, cancellationToken);
                }

                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                while (tcpClient.Available == 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    await Task.Delay(10, cancellationToken);
                }

                PacketHandler packetHandler = new(tcpClient.GetStream(), cancellationToken);
                HandshakePacket handshakePacket = new(0x00, (await packetHandler.ReadNextPacketAsync()).Data);
                Logger.Information("Handshake Packet Received,\n\tProtocol Version: {version},\n\tServer Address: {address},\n\tPort: {port},\n\tNext State: {state}", handshakePacket.ProtocolVersion, handshakePacket.ServerAddress, handshakePacket.ServerAddress, handshakePacket.NextClientState);

                Packet requestPacket = await packetHandler.ReadNextPacketAsync();
                Logger.Information("Request Packet Received,\n\tId: {id}\n\tData: null", requestPacket.Id);

                ResponsePacket responsePacket = new(new ServerStatus());
                await packetHandler.WritePacketAsync(responsePacket);

                PingPongPacket pingPacket = new(0x01, (await packetHandler.ReadNextPacketAsync()).Data);
                Packet pongPacket = new(0x01, pingPacket.Data);
                await packetHandler.WritePacketAsync(pongPacket);
                Logger.Information("Ping Packet Received,\n\tPacket Id: {id}\n\tPacket Data: {data}", pingPacket.Id, pingPacket.Payload);
            }
        }
    }
}