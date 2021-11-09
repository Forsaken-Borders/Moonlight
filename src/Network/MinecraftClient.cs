using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Web;
using Moonlight.Network.Packets;
using Moonlight.Types;
using Serilog;

namespace Moonlight.Network
{
    public class MinecraftClient : IDisposable
    {
        public PacketHandler PacketHandler { get; init; }
        public bool LocalhostConnection { get; init; }
        private ILogger Logger { get; init; } = Program.Logger.ForContext<MinecraftClient>();

        public MinecraftClient(TcpClient tcpClient)
        {
            PacketHandler = new(tcpClient.GetStream());
            LocalhostConnection = (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() == "127.0.0.1";
        }

        public bool Login()
        {
            Logger.Verbose("Sending a Login Start packet...");
            LoginStartPacket loginStartPacket = new(PacketHandler.ReadNextPacket().Data);
            Logger.Debug("{username} is attempting to login...", loginStartPacket.Username);
            if (!LocalhostConnection)
            {
                EncryptionRequestPacket encryptionRequestPacket = new();
                PacketHandler.WritePacket(encryptionRequestPacket);
                Packet unknownPacket = PacketHandler.ReadNextPacket();
                EncryptionResponsePacket encryptionResponsePacket = new(unknownPacket.Data);
                if (!encryptionRequestPacket.VerifyToken.SequenceEqual(encryptionResponsePacket.VerifyToken))
                {
                    Logger.Warning("Client Error: {username} failed to encrypt the packets sucessfully (failed on token verification). This either means they're using hacks, using a mod that changes the Minecraft protocol or is attempting to make their own Minecraft client from scratch. This is highly irregular and should be proceeded with caution.");
                    PacketHandler.WritePacket(new DisconnectPacket("Client Error: Failed to do encrypted token verification. Please see https://wiki.vg/Protocol_Encryption and https://wiki.vg/Protocol#Encryption_Response for documentation."));
                    PacketHandler.Dispose();
                    return false;
                }

                PacketHandler.EnableEncryption(encryptionResponsePacket.SharedSecret);
            }
            HttpClient httpClient = new();

            MojangSessionServerResponse mojangSessionServerResponse = httpClient.GetFromJsonAsync<MojangSessionServerResponse>(new Uri($"https://sessionserver.mojang.com/session/minecraft/hasJoined?username={HttpUtility.UrlEncode(Encoding.UTF8.GetBytes(loginStartPacket.Username.MinecraftShaDigest()))}&serverId={HttpUtility.UrlEncode(Encoding.UTF8.GetBytes(EncryptionRequestPacket.StaticServerId.MinecraftShaDigest()))}")).GetAwaiter().GetResult();
            LoginSuccessPacket loginSuccessPacket = new(mojangSessionServerResponse);
            PacketHandler.WritePacket(loginSuccessPacket);
            Logger.Information("{username} has successfully logged in.", loginStartPacket.Username);

            return true;
        }

        public void Dispose()
        {
            PacketHandler.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}