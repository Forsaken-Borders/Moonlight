using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moonlight.Api.Net;
using Moonlight.Protocol.Net;

[assembly: InternalsVisibleTo("Moonlight")]
namespace Moonlight.Api
{
    public sealed class Server
    {
        public ServerConfiguration Configuration { get; init; }
        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        private readonly ILogger<Server> _logger;
        private readonly ILoggerFactory _loggerProvider;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public Server(ServerConfiguration configuration, ILoggerFactory? logger = null)
        {
            Configuration = configuration;
            _loggerProvider = logger ?? NullLoggerFactory.Instance;
            _logger = _loggerProvider.CreateLogger<Server>();
        }

        public async Task StartAsync()
        {
            _logger.LogInformation("Starting server...");
            TcpListener listener = new(IPAddress.Parse(Configuration.Host), Configuration.Port);
            listener.Start();

            _logger.LogInformation("Server started on {EndPoint}", listener.LocalEndpoint);
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(CancellationToken);
                _logger.LogInformation("Client connected: {EndPoint}", client.Client.RemoteEndPoint);

                // Try to read the handshake packet
                PacketReader reader = new(client.GetStream(), _loggerProvider.CreateLogger<PacketReader>());
                HandshakePacket handshake;

                try
                {
                    handshake = await reader.ReadPacketAsync<HandshakePacket>(CancellationToken);
                    _logger.LogInformation("Handshake received: {Handshake}", handshake);
                }
                catch (InvalidOperationException error)
                {
                    _logger.LogError(error, "Failed to read handshake packet.");
                    continue;
                }

                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    // Try to read the next packet
                    IPacket packet;

                    try
                    {
                        packet = await reader.ReadPacketAsync(CancellationToken);
                        _logger.LogInformation("Packet received: {Packet}", packet);
                    }
                    catch (InvalidOperationException error)
                    {
                        _logger.LogError(error, "Failed to read packet.");
                        break;
                    }
                }
            }
        }
    }
}
