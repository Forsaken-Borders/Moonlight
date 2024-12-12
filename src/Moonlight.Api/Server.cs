using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moonlight.Api.Events;
using Moonlight.Api.Events.EventArgs;
using Moonlight.Api.Net;
using Moonlight.Protocol.Net;

namespace Moonlight.Api
{
    public sealed class Server
    {
        public ServerConfiguration Configuration { get; init; }
        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        private readonly ILogger<Server> _logger;
        private readonly PacketReaderFactory _packetReaderFactory;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly AsyncServerEvent<PacketReceivedAsyncServerEventArgs> _packetReceivedServerEvent;

        public Server(ServerConfiguration serverConfiguration, PacketReaderFactory packetReaderFactory, ILogger<Server> logger, AsyncServerEvent<PacketReceivedAsyncServerEventArgs> packetReceivedServerEvent)
        {
            Configuration = serverConfiguration;
            _packetReaderFactory = packetReaderFactory;
            _logger = logger;
            _packetReceivedServerEvent = packetReceivedServerEvent;
        }

        public async Task StartAsync()
        {
            _packetReaderFactory.Prepare();

            _logger.LogInformation("Starting server...");
            TcpListener listener = new(IPAddress.Parse(Configuration.Host), Configuration.Port);
            listener.Start();

            _logger.LogInformation("Server started on {EndPoint}", listener.LocalEndpoint);
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                _ = HandleClientAsync(await listener.AcceptTcpClientAsync(CancellationToken));
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            _logger.LogInformation("Client connected: {EndPoint}", client.Client.RemoteEndPoint);

            using NetworkStream stream = client.GetStream();
            PacketReader reader = _packetReaderFactory.CreatePacketReader(stream);
            try
            {
                HandshakePacket handshake = await reader.ReadPacketAsync<HandshakePacket>(CancellationToken);
                if (await _packetReceivedServerEvent.InvokePreHandlersAsync(new PacketReceivedAsyncServerEventArgs(handshake, reader)))
                {
                    await _packetReceivedServerEvent.InvokePostHandlersAsync(new PacketReceivedAsyncServerEventArgs(handshake, reader));
                }
            }
            catch (InvalidDataException error)
            {
                _logger.LogError(error, "Error handling client: {EndPoint}", client.Client.RemoteEndPoint);
                return;
            }

            while (!CancellationToken.IsCancellationRequested)
            {
                IPacket packet = await reader.ReadPacketAsync(CancellationToken);
                if (await _packetReceivedServerEvent.InvokePreHandlersAsync(new PacketReceivedAsyncServerEventArgs(packet, reader)))
                {
                    await _packetReceivedServerEvent.InvokePostHandlersAsync(new PacketReceivedAsyncServerEventArgs(packet, reader));
                }
            }
        }
    }
}
