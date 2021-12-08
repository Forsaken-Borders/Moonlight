using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Moonlight.Api.Mojang;
using Moonlight.Api.Networking.Packets;
using Moonlight.Api.Networking.Packets.LoginState;
using Moonlight.Api.Types.Chat;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Serilog;

namespace Moonlight.Api.Networking
{
    public class MinecraftClient : IDisposable
    {
        public PacketHandler PacketHandler { get; init; }
        public bool LocalhostConnection { get; init; }
        public AsymmetricCipherKeyPair Keys { get; init; }

        private ILogger Logger { get; init; }
        private IConfiguration Configuration { get; init; }
        private CancellationToken CancellationToken { get; init; }

        public MinecraftClient(Stream stream, bool localhostConnection, ILogger logger, IConfiguration configuration, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));
            ArgumentNullException.ThrowIfNull(localhostConnection, nameof(localhostConnection));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
            ArgumentNullException.ThrowIfNull(cancellationToken, nameof(cancellationToken));

            PacketHandler = new PacketHandler(stream, cancellationToken);
            LocalhostConnection = localhostConnection;
            RsaKeyPairGenerator rsaKeyPairGenerator = new();
            rsaKeyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 1024));
            Keys = rsaKeyPairGenerator.GenerateKeyPair();

            Logger = logger;
            Configuration = configuration;
            CancellationToken = cancellationToken;
        }

        public MinecraftClient(TcpClient tcpClient, ILogger logger, IConfiguration configuration, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(tcpClient, nameof(tcpClient));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
            ArgumentNullException.ThrowIfNull(cancellationToken, nameof(cancellationToken));

            PacketHandler = new PacketHandler(tcpClient.GetStream(), cancellationToken);
            LocalhostConnection = (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() == "127.0.0.1";
            RsaKeyPairGenerator rsaKeyPairGenerator = new();
            rsaKeyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 1024));
            Keys = rsaKeyPairGenerator.GenerateKeyPair();

            Logger = logger;
            Configuration = configuration;
            CancellationToken = cancellationToken;
        }

        public async Task Login()
        {
            Logger.Verbose("Sending a Login Start packet...");
            LoginStartPacket loginStartPacket = PacketHandler.ReadPacket<LoginStartPacket>();
            Logger.Debug("{username} is attempting to login...", loginStartPacket.Username);

            // If the connection isn't localhost, MC protocol requires us to enable encryption.
            // https://wiki.vg/Protocol_Encryption
            if (!LocalhostConnection)
            {
                EncryptionRequestPacket encryptionRequestPacket = new(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(Keys.Public).ToAsn1Object().GetDerEncoded(), RandomNumberGenerator.GetBytes(4));
                PacketHandler.WritePacket(encryptionRequestPacket);
                Packet unknownPacket = PacketHandler.ReadPacket<EncryptionResponsePacket>();
                EncryptionResponsePacket encryptionResponsePacket = new(unknownPacket.Data!, Keys.Private);

                // SequenceEqual should be used when checking if byte arrays are the same, considering `==` and `.Equals` apparently say they aren't.
                if (!encryptionRequestPacket.VerifyToken.SequenceEqual(encryptionResponsePacket.VerifyToken))
                {
                    Logger.Warning("Client Error: {username} failed to encrypt the packets sucessfully (failed on token verification). This either means they're using hacks, using a mod that changes the Minecraft protocol or is attempting to make their own Minecraft client from scratch. **This is highly irregular and should be proceeded with caution.**");
                    PacketHandler.WritePacket(new DisconnectPacket("Client Error: Failed to correctly implement encrypted token verification. Please see https://wiki.vg/Protocol_Encryption and https://wiki.vg/Protocol#Encryption_Response for documentation."));
                    PacketHandler.Dispose();
                }

                PacketHandler.Stream.EnableEncryption(encryptionResponsePacket.SharedSecret);

                IEnumerable<byte> serverId = Encoding.ASCII.GetBytes(Enumerable.Range(0, 10).Select(character => (char)Random.Shared.Next('A', 'z' + 1)).ToArray());
                serverId = serverId.Concat(encryptionResponsePacket.SharedSecret);
                serverId = serverId.Concat(encryptionRequestPacket.PublicKey);

                SessionServerResponse? mojangSessionServerResponse = await MinecraftAPI.HasJoined(loginStartPacket.Username, serverId, CancellationToken);
                if (mojangSessionServerResponse == null)
                {
                    PacketHandler.WritePacket(new DisconnectPacket("&cSession Servers failed to respond!"));
                    Dispose();
                    return;
                }
                LoginSuccessPacket loginSuccessPacket = new(mojangSessionServerResponse);
                PacketHandler.WritePacket(loginSuccessPacket);
                Logger.Information("{username} has successfully logged in.", loginStartPacket.Username);
            }

            CancellationToken.Register(() =>
            {
                Disconnect(Configuration.GetValue("server:shutdown_message", "The server is shutting down!"));
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