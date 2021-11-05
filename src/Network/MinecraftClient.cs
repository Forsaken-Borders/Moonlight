using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using Moonlight.Network.Packets;
using Moonlight.Types;
using Serilog;

namespace Moonlight.Network
{
    public class MinecraftClient : IDisposable
    {
        public PacketHandler PacketHandler { get; init; }
        private ILogger Logger { get; init; } = Program.Logger.ForContext<MinecraftClient>();

        public MinecraftClient(TcpClient tcpClient) => PacketHandler = new(tcpClient.GetStream());

        public bool Login()
        {
            Logger.Verbose("Sending a Login Start packet...");
            LoginStartPacket loginStartPacket = new(PacketHandler.ReadNextPacket().Data);
            Logger.Debug("{username} is attempting to login...", loginStartPacket.Username);
            EncryptionRequestPacket encryptionRequestPacket = new();
            PacketHandler.WritePacket(encryptionRequestPacket);
            Packet unknownPacket = PacketHandler.ReadNextPacket();
            EncryptionResponsePacket encryptionResponsePacket = new(unknownPacket.Data);
            if (encryptionRequestPacket.VerifyToken != encryptionResponsePacket.VerifyToken)
            {
                Logger.Warning("Client Error: {username} failed to encrypt the packets sucessfully (failed on token verification). This either means they're using hacks, using a mod that changes the Minecraft protocol or is attempting to make their own Minecraft client from scratch. This is highly irregular and should be proceeded with caution.");
                PacketHandler.WritePacket(new DisconnectPacket("Client Error: Failed to do encrypted token verification. Please see https://wiki.vg/Protocol_Encryption and https://wiki.vg/Protocol#Encryption_Response for documentation."));
                PacketHandler.Dispose();
                return false;
            }

            PacketHandler.EnableEncryption(encryptionResponsePacket.SharedSecret);
            Logger.Information("{username} has successfully logged in.", loginStartPacket.Username);
            HttpClient httpClient = new();
            LoginSuccessPacket loginSuccessPacket = new(httpClient.GetFromJsonAsync<MojangSessionServerResponse>(new Uri($"https://sessionserver.mojang.com/session/minecraft/hasJoined?username={loginStartPacket.Username}&serverId={loginStartPacket.Username.MinecraftShaDigest()}")).GetAwaiter().GetResult());
            PacketHandler.WritePacket(loginSuccessPacket);

            return true;
        }

        public void Dispose()
        {
            PacketHandler.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}