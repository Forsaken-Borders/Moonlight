using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Moonlight")]
namespace Moonlight.Api
{
    public sealed class Server
    {
        public ServerConfiguration Configuration { get; init; }
        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        internal Server(ServerConfiguration configuration) => Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        internal Task StartAsync()
        {
            TcpListener listener = new(IPAddress.Parse(Configuration.Host), Configuration.Port);
            listener.Start();
            _ = ServerLoopAsync(listener);
            return Task.CompletedTask;
        }

        internal async Task ServerLoopAsync(TcpListener listener)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(CancellationToken);
            }
        }
    }
}
