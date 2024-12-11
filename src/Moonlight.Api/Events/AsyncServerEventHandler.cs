using System.Threading.Tasks;

namespace Moonlight.Api.Events
{
    public delegate ValueTask AsyncServerEventHandler(AsyncServerEventArgs eventArgs);
    public delegate ValueTask AsyncServerEventHandler<T>(T eventArgs) where T : AsyncServerEventArgs;
}
