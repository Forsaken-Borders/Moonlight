using System.Threading.Tasks;

namespace Moonlight.Api.Events
{
    public delegate ValueTask<bool> AsyncServerEventPreHandler(AsyncServerEventArgs eventArgs);
    public delegate ValueTask<bool> AsyncServerEventPreHandler<T>(T eventArgs) where T : AsyncServerEventArgs;
}
