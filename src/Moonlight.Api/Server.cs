using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Moonlight")]
namespace Moonlight.Api
{
    public sealed class Server
    {
        internal Server() { }
        public Task StartAsync() => Task.CompletedTask;
    }
}
