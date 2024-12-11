using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moonlight.Api.Events
{
    public sealed record AsyncServerEvent<TEventArgs> where TEventArgs : AsyncServerEventArgs
    {
        public IReadOnlyDictionary<AsyncServerEventPreHandler<TEventArgs>, AsyncServerEventPriority> PreHandlers => _preHandlers;
        public IReadOnlyDictionary<AsyncServerEventHandler<TEventArgs>, AsyncServerEventPriority> PostHandlers => _postHandlers;

        private readonly Dictionary<AsyncServerEventPreHandler<TEventArgs>, AsyncServerEventPriority> _preHandlers = [];
        private readonly Dictionary<AsyncServerEventHandler<TEventArgs>, AsyncServerEventPriority> _postHandlers = [];

        private AsyncServerEventPreHandler<TEventArgs> _preEventHandlerDelegate = _ => new ValueTask<bool>(true);
        private AsyncServerEventHandler<TEventArgs> _postEventHandlerDelegate = _ => ValueTask.CompletedTask;

        public void AddPreHandler(AsyncServerEventPreHandler<TEventArgs> handler, AsyncServerEventPriority priority = AsyncServerEventPriority.Normal) => _preHandlers.Add(handler, priority);
        public void AddPostHandler(AsyncServerEventHandler<TEventArgs> handler, AsyncServerEventPriority priority = AsyncServerEventPriority.Normal) => _postHandlers.Add(handler, priority);

        public bool RemovePreHandler(AsyncServerEventPreHandler<TEventArgs> handler) => _preHandlers.Remove(handler);
        public bool RemovePostHandler(AsyncServerEventHandler<TEventArgs> handler) => _postHandlers.Remove(handler);

        public ValueTask<bool> InvokePreHandlersAsync(TEventArgs eventArgs) => _preEventHandlerDelegate(eventArgs);
        public ValueTask InvokePostHandlersAsync(TEventArgs eventArgs) => _postEventHandlerDelegate(eventArgs);

        public void Prepare()
        {
            List<AsyncServerEventPreHandler<TEventArgs>> preHandlers = _preHandlers.OrderBy(x => x.Value).Select(x => x.Key).ToList();
            List<AsyncServerEventHandler<TEventArgs>> postHandlers = _postHandlers.OrderBy(x => x.Value).Select(x => x.Key).ToList();

            _preEventHandlerDelegate = preHandlers.Count switch
            {
                0 => _ => new ValueTask<bool>(true),
                1 => preHandlers.First(),
                2 => async ValueTask<bool> (TEventArgs eventArgs) => await preHandlers[0](eventArgs) && await preHandlers[1](eventArgs),
                _ => async eventArgs =>
                {
                    bool result = true;
                    foreach (AsyncServerEventPreHandler<TEventArgs> handler in preHandlers)
                    {
                        result &= await handler(eventArgs);
                    }

                    return result;
                }
            };

            _postEventHandlerDelegate = _postHandlers.Count switch
            {
                0 => _ => ValueTask.CompletedTask,
                1 => postHandlers.First(),
                2 => async ValueTask (TEventArgs eventArgs) =>
                {
                    await postHandlers[0](eventArgs);
                    await postHandlers[1](eventArgs);
                }
                ,
                _ => async eventArgs => await Parallel.ForEachAsync(postHandlers, async (handler, cancellationToken) => await handler(eventArgs))
            };
        }
    }
}
