using Microsoft.Extensions.Configuration;
using Moonlight.Api.Minecraft.Abstractions.Networking;
using Moonlight.Api.Minecraft.Objects.Chat;
using Moonlight.Api.Minecraft.Objects.Networking.Packets.LoginState;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Serilog;

namespace Moonlight.Api.Minecraft.Objects.Networking
{
    public class MinecraftClient : IMinecraftClient
    {
        /// <inheritdoc />
        public IPacketHandler PacketHandler { get; init; }

        /// <inheritdoc />
        public AsymmetricCipherKeyPair KeyPair { get; private set; }

        /// <inheritdoc />
        public bool IsLocalhost { get; init; }

        /// <inheritdoc />
        public bool IsConnected => PacketHandler?.IsDisposed ?? false;

        private ILogger Logger { get; init; }
        private IConfiguration Configuration { get; init; }
        private CancellationToken CancellationToken { get; init; }

        public MinecraftClient(Stream stream, bool isLocalhost, ILogger logger, IConfiguration configuration, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));
            ArgumentNullException.ThrowIfNull(isLocalhost, nameof(isLocalhost));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
            ArgumentNullException.ThrowIfNull(cancellationToken, nameof(cancellationToken));

            RsaKeyPairGenerator rsaKeyPairGenerator = new();
            rsaKeyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 1024));
            KeyPair = rsaKeyPairGenerator.GenerateKeyPair();

            PacketHandler = new PacketHandler(stream, cancellationToken);
            IsLocalhost = isLocalhost;
            Logger = logger;
            Configuration = configuration;
            CancellationToken = cancellationToken;
            CancellationToken.Register(async () =>
            {
                await DisconnectAsync(Configuration.GetValue("server:shutdown_message", "&cServer shutting down!"));
                PacketHandler.Dispose();
            });
        }

        /// <inheritdoc />
        public async Task DisconnectAsync(ChatComponent? reason = null)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Cannot disconnect from the server if the client is not connected.");
            }

            await PacketHandler.WritePacketAsync(reason == null ? new DisconnectPacket() : new DisconnectPacket(reason));
            await PacketHandler.DisposeAsync();
            Logger.Information("Disconnected user from the server"); // TODO: Add who and why.
        }

        /// <inheritdoc />
        public Task Login() => throw new NotImplementedException();

        /// <inheritdoc />
        public void Dispose()
        {
            PacketHandler.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await PacketHandler.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}