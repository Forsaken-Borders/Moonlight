using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Moonlight.Api.Events
{
    public sealed record AsyncServerEvent<TEventArgs> where TEventArgs : AsyncServerEventArgs
    {
        public IReadOnlyDictionary<AsyncServerEventPreHandler<TEventArgs>, AsyncServerEventPriority> PreHandlers => _preHandlers;
        public IReadOnlyDictionary<AsyncServerEventHandler<TEventArgs>, AsyncServerEventPriority> PostHandlers => _postHandlers;

        private readonly Dictionary<AsyncServerEventPreHandler<TEventArgs>, AsyncServerEventPriority> _preHandlers = [];
        private readonly Dictionary<AsyncServerEventHandler<TEventArgs>, AsyncServerEventPriority> _postHandlers = [];

        private AsyncServerEventPreHandler<TEventArgs> _preEventHandlerDelegate;
        private AsyncServerEventHandler<TEventArgs> _postEventHandlerDelegate;
        private readonly IConfiguration _configuration;

        public AsyncServerEvent(IConfiguration configuration)
        {
            _configuration = configuration;
            _preEventHandlerDelegate = LazyPreHandler;
            _postEventHandlerDelegate = LazyPostHandler;
        }

        public void AddPreHandler(AsyncServerEventPreHandler<TEventArgs> handler, AsyncServerEventPriority priority = AsyncServerEventPriority.Normal) => _preHandlers.Add(handler, priority);
        public void AddPostHandler(AsyncServerEventHandler<TEventArgs> handler, AsyncServerEventPriority priority = AsyncServerEventPriority.Normal) => _postHandlers.Add(handler, priority);

        public bool RemovePreHandler(AsyncServerEventPreHandler<TEventArgs> handler) => _preHandlers.Remove(handler);
        public bool RemovePostHandler(AsyncServerEventHandler<TEventArgs> handler) => _postHandlers.Remove(handler);

        public async ValueTask<bool> InvokeAsync(TEventArgs eventArgs)
        {
            if (await InvokePreHandlersAsync(eventArgs))
            {
                await InvokePostHandlersAsync(eventArgs);
                return true;
            }

            return false;
        }

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
            else if (!_configuration.GetValue("Moonlight:Events:Parallelize", false) || preHandlers.Count < _configuration.GetValue("Moonlight:Events:MinParallelHandlers", Environment.ProcessorCount))
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
            else if (!_configuration.GetValue("Moonlight:Events:Parallelize", false) || postHandlers.Count < _configuration.GetValue("Moonlight:Events:MinParallelHandlers", Environment.ProcessorCount))
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

        private static ValueTask<bool> EmptyPreHandler(TEventArgs _) => ValueTask.FromResult(true);
        private static ValueTask EmptyPostHandler(TEventArgs _) => ValueTask.CompletedTask;

        private ValueTask<bool> LazyPreHandler(TEventArgs eventArgs)
        {
            Prepare();
            return _preEventHandlerDelegate(eventArgs);
        }

        private ValueTask LazyPostHandler(TEventArgs eventArgs)
        {
            Prepare();
            return _postEventHandlerDelegate(eventArgs);
        }

        public override string ToString() => $"{GetType()}, PreHandlers: {_preHandlers.Count}, PostHandlers: {_postHandlers.Count}";
    }
}
