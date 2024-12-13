using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

namespace Moonlight.Api.Events
{
    public sealed class AsyncServerEventContainer
    {
        private readonly Dictionary<Type, object> _serverEvents = [];
        private readonly Dictionary<Type, List<(AsyncServerEventPreHandler, AsyncServerEventPriority)>> _preHandlers = [];
        private readonly Dictionary<Type, List<(AsyncServerEventHandler, AsyncServerEventPriority)>> _postHandlers = [];
        private readonly IConfiguration _configuration;

        public AsyncServerEventContainer(IConfiguration configuration) => _configuration = configuration;

        public AsyncServerEvent<T> GetAsyncServerEvent<T>() where T : AsyncServerEventArgs
        {
            if (_serverEvents.TryGetValue(typeof(T), out object? value))
            {
                return (AsyncServerEvent<T>)value;
            }

            AsyncServerEvent<T> asyncServerEvent = new(_configuration);
            if (_preHandlers.TryGetValue(typeof(T), out List<(AsyncServerEventPreHandler, AsyncServerEventPriority)>? preHandlers))
            {
                foreach ((AsyncServerEventPreHandler preHandler, AsyncServerEventPriority priority) in preHandlers)
                {
                    // Cannot use 'preHandler' as a ref or out value because it is a 'foreach iteration variable' csharp(CS1657)
                    AsyncServerEventPreHandler localPreHandler = preHandler;
                    asyncServerEvent.AddPreHandler(Unsafe.As<AsyncServerEventPreHandler, AsyncServerEventPreHandler<T>>(ref localPreHandler), priority);
                }
            }

            if (_postHandlers.TryGetValue(typeof(T), out List<(AsyncServerEventHandler, AsyncServerEventPriority)>? postHandlers))
            {
                foreach ((AsyncServerEventHandler postHandler, AsyncServerEventPriority priority) in postHandlers)
                {
                    // Cannot use 'postHandler' as a ref or out value because it is a 'foreach iteration variable' csharp(CS1657)
                    AsyncServerEventHandler localPostHandler = postHandler;
                    asyncServerEvent.AddPostHandler(Unsafe.As<AsyncServerEventHandler, AsyncServerEventHandler<T>>(ref localPostHandler), priority);
                }
            }

            asyncServerEvent.Prepare();
            _serverEvents.Add(typeof(T), asyncServerEvent);
            return asyncServerEvent;
        }

        public void AddPreHandler<T>(AsyncServerEventPreHandler<T> preHandler, AsyncServerEventPriority priority) where T : AsyncServerEventArgs
        {
            if (!_preHandlers.TryGetValue(typeof(T), out List<(AsyncServerEventPreHandler, AsyncServerEventPriority)>? preHandlers))
            {
                preHandlers = [];
                _preHandlers.Add(typeof(T), preHandlers);
            }

            preHandlers.Add((Unsafe.As<AsyncServerEventPreHandler<T>, AsyncServerEventPreHandler>(ref preHandler), priority));
        }

        public void AddPreHandler(Type type, AsyncServerEventPreHandler preHandler, AsyncServerEventPriority priority)
        {
            if (type.IsAssignableFrom(typeof(AsyncServerEventArgs)))
            {
                throw new ArgumentException("Type must be a subclass of AsyncServerEventArgs", nameof(type));
            }

            if (!_preHandlers.TryGetValue(type, out List<(AsyncServerEventPreHandler, AsyncServerEventPriority)>? preHandlers))
            {
                preHandlers = [];
                _preHandlers.Add(type, preHandlers);
            }

            preHandlers.Add((preHandler, priority));
        }

        public void AddPostHandler<T>(AsyncServerEventHandler<T> postHandler, AsyncServerEventPriority priority) where T : AsyncServerEventArgs
        {
            if (!_postHandlers.TryGetValue(typeof(T), out List<(AsyncServerEventHandler, AsyncServerEventPriority)>? postHandlers))
            {
                postHandlers = [];
                _postHandlers.Add(typeof(T), postHandlers);
            }

            postHandlers.Add((Unsafe.As<AsyncServerEventHandler<T>, AsyncServerEventHandler>(ref postHandler), priority));
        }

        public void AddPostHandler(Type type, AsyncServerEventHandler postHandler, AsyncServerEventPriority priority)
        {
            if (type.IsAssignableFrom(typeof(AsyncServerEventArgs)))
            {
                throw new ArgumentException("Type must be a subclass of AsyncServerEventArgs", nameof(type));
            }

            if (!_postHandlers.TryGetValue(type, out List<(AsyncServerEventHandler, AsyncServerEventPriority)>? postHandlers))
            {
                postHandlers = [];
                _postHandlers.Add(type, postHandlers);
            }

            postHandlers.Add((postHandler, priority));
        }
    }
}
