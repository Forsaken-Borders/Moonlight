using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        [SuppressMessage("Roslyn", "IDE0045", Justification = "Ternary rabbit hole.")]
        public void Prepare()
        {
            List<AsyncServerEventPreHandler<TEventArgs>> preHandlers = _preHandlers.OrderBy(x => x.Value).Select(x => x.Key).ToList();
            List<AsyncServerEventHandler<TEventArgs>> postHandlers = _postHandlers.OrderBy(x => x.Value).Select(x => x.Key).ToList();

            if (preHandlers.Count == 0)
            {
                _preEventHandlerDelegate = EmptyPreHandler;
            }
            else if (preHandlers.Count == 1)
            {
                _preEventHandlerDelegate = preHandlers[0];
            }
            else if (preHandlers.Count == 2)
            {
                _preEventHandlerDelegate = async ValueTask<bool> (TEventArgs eventArgs) => await preHandlers[0](eventArgs) && await preHandlers[1](eventArgs);
            }
            else if (preHandlers.Count < Math.Min(Environment.ProcessorCount, 8))
            {
                _preEventHandlerDelegate = async eventArgs =>
                {
                    bool result = true;
                    foreach (AsyncServerEventPreHandler<TEventArgs> handler in preHandlers)
                    {
                        result &= await handler(eventArgs);
                    }

                    return result;
                };
            }
            else
            {
                _preEventHandlerDelegate = async (TEventArgs eventArgs) =>
                {
                    bool result = true;
                    await Parallel.ForEachAsync(preHandlers, async (handler, cancellationToken) => result &= await handler(eventArgs));
                    return result;
                };
            }

            if (postHandlers.Count == 0)
            {
                _postEventHandlerDelegate = EmptyPostHandler;
            }
            else if (postHandlers.Count == 1)
            {
                _postEventHandlerDelegate = postHandlers[0];
            }
            else if (postHandlers.Count < Math.Min(Environment.ProcessorCount, 8))
            {
                _postEventHandlerDelegate = async (TEventArgs eventArgs) =>
                {
                    foreach (AsyncServerEventHandler<TEventArgs> handler in postHandlers)
                    {
                        await handler(eventArgs);
                    }
                };
            }
            else
            {
                _postEventHandlerDelegate = async (TEventArgs eventArgs) =>
                    await Parallel.ForEachAsync(postHandlers, async (handler, cancellationToken) => await handler(eventArgs));
            }
        }

        private ValueTask<bool> EmptyPreHandler(TEventArgs _) => ValueTask.FromResult(true);
        private ValueTask EmptyPostHandler(TEventArgs _) => ValueTask.CompletedTask;

        public override string ToString() => $"{PreHandlers.Count}, {PostHandlers.Count}";
    }
}
