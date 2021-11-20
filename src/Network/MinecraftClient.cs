using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using Microsoft.Extensions.Configuration;
using Moonlight.Network.Packets;
using Moonlight.Types;
using Moonlight.Types.Chat;
using Serilog;

namespace Moonlight.Network
{
    public class MinecraftClient : IDisposable
    {
        public PacketHandler PacketHandler { get; init; }
        public bool LocalhostConnection { get; init; }
        private ILogger Logger { get; init; } = Server.Logger.ForContext<MinecraftClient>();
        private CancellationToken CancellationToken { get; init; }

        public MinecraftClient(Stream stream, bool localhostConnection, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));
            ArgumentNullException.ThrowIfNull(localhostConnection, nameof(localhostConnection));
            ArgumentNullException.ThrowIfNull(cancellationToken, nameof(cancellationToken));

            PacketHandler = new(stream, cancellationToken);
            LocalhostConnection = localhostConnection;
            CancellationToken = cancellationToken;
        }

        public MinecraftClient(TcpClient tcpClient, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tcpClient, nameof(tcpClient));
            ArgumentNullException.ThrowIfNull(cancellationToken, nameof(cancellationToken));

            PacketHandler = new(tcpClient.GetStream(), cancellationToken);
            LocalhostConnection = (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() == "127.0.0.1";
            CancellationToken = cancellationToken;
        }

        public void Login()
        {
            Logger.Verbose("Sending a Login Start packet...");
            LoginStartPacket loginStartPacket = new(PacketHandler.ReadNextPacket().Data);
            Logger.Debug("{username} is attempting to login...", loginStartPacket.Username);

            // If the connection isn't localhost, MC protocol requires us to enable encryption.
            // wiki.vg/Protocol_Encryption
            if (!LocalhostConnection)
            {
                PacketHandler.GenerateKeys();

                EncryptionRequestPacket encryptionRequestPacket = new(PacketHandler.Keys.Public.ToDerFormat(), RandomNumberGenerator.GetBytes(4));
                PacketHandler.WritePacket(encryptionRequestPacket);
                Packet unknownPacket = PacketHandler.ReadNextPacket();
                EncryptionResponsePacket encryptionResponsePacket = new(unknownPacket.Data, PacketHandler.Keys.Private);

                // SequenceEqual should be used when checking if byte arrays are the same, considering `==` and `.Equals` apparently say they aren't.
                if (!encryptionRequestPacket.VerifyToken.SequenceEqual(encryptionResponsePacket.VerifyToken))
                {
                    Logger.Warning("Client Error: {username} failed to encrypt the packets sucessfully (failed on token verification). This either means they're using hacks, using a mod that changes the Minecraft protocol or is attempting to make their own Minecraft client from scratch. **This is highly irregular and should be proceeded with caution.**");
                    PacketHandler.WritePacket(new DisconnectPacket("Client Error: Failed to correctly implement encrypted token verification. Please see https://wiki.vg/Protocol_Encryption and https://wiki.vg/Protocol#Encryption_Response for documentation."));
                    PacketHandler.Dispose();
                }

                PacketHandler.EnableEncryption(encryptionResponsePacket.SharedSecret);

                IEnumerable<byte> serverId = Encoding.ASCII.GetBytes(EncryptionRequestPacket.StaticServerId);
                serverId = serverId.Concat(encryptionResponsePacket.SharedSecret);
                serverId = serverId.Concat(encryptionRequestPacket.PublicKey);

                HttpClient httpClient = new();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Moonlight/1.0");
                MojangSessionServerResponse mojangSessionServerResponse = httpClient.GetFromJsonAsync<MojangSessionServerResponse>($"https://sessionserver.mojang.com/session/minecraft/hasJoined?username={HttpUtility.UrlEncode(loginStartPacket.Username)}&serverId={HttpUtility.UrlEncode(serverId.ToArray().MinecraftShaDigest())}").GetAwaiter().GetResult();
                LoginSuccessPacket loginSuccessPacket = new(mojangSessionServerResponse);
                PacketHandler.WritePacket(loginSuccessPacket);
                Logger.Information("{username} has successfully logged in.", loginStartPacket.Username);
            }

            CancellationToken.Register(() =>
            {
                Disconnect(Server.Configuration.GetValue("server:shutdown_message", "The server is shutting down!"));
                Dispose();
            });
        }

        public void Disconnect(ChatComponent reason) => PacketHandler.WritePacket(new DisconnectPacket(reason));

        public void Dispose()
        {
            PacketHandler.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}