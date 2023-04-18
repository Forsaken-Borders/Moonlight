using System.Threading.Tasks;
using Moonlight.Api;

namespace Moonlight
{
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            Server server = new();
            await server.StartAsync();
        }
    }
}
