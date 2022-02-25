using Microsoft.Extensions.DependencyInjection;
using Moonlight.Api.Server.Abstractions;
using Moonlight.Api.Server.Attributes;
using Moonlight.Api.Server.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Moonlight.Server.Objects
{
    public class EventManager : IEventManager
    {
        public IReadOnlyList<IEventSubscription> EventSubscriptions => InternalEventSubscriptions.AsReadOnly();
        internal readonly List<IEventSubscription> InternalEventSubscriptions = new();
        private ServiceProvider ServiceProvider { get; }

        internal EventManager(ServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
            ServiceProvider = serviceProvider;
        }

        public void Subscribe(IEventSubscription subscription) => InternalEventSubscriptions.Add(subscription);
        public void Unsubscribe(IEventSubscription subscription) => InternalEventSubscriptions.Remove(subscription);

        internal async Task ExecuteAsync(EventName eventName, object[]? eventArgs = null)
        {
            foreach (IEventSubscription subscription in InternalEventSubscriptions)
            {
                if (subscription.EventName == eventName)
                {
                    List<object> methodParameters = new();
                    foreach (ParameterInfo parameter in subscription.EventHandler.GetParameters())
                    {
                        if (!(eventArgs?.Any(eventArgType => eventArgType.GetType() != parameter.GetType()) ?? false))
                        {
                            methodParameters.Add(ServiceProvider.GetService(parameter.GetType()));
                        }
                    }
                    await (Task)(subscription.EventHandler.CreateDelegate(subscription.EventHandler.GetType()).DynamicInvoke(eventArgs, methodParameters)!);
                }
            }
        }

        internal void RegisterEventSubscriptions(IEnumerable<Plugin> plugins)
        {
            foreach (Plugin plugin in plugins)
            {
                foreach (Type exportedType in plugin.GetType().Assembly.GetExportedTypes())
                {
                    foreach (MethodInfo methodInfo in exportedType.GetRuntimeMethods())
                    {
                        foreach (SubscribeToEventAttribute attribute in methodInfo.GetCustomAttributes<SubscribeToEventAttribute>())
                        {
                            EventSubscription subscription = new(attribute.EventName, attribute.Priority, methodInfo, plugin);
                            Subscribe(subscription);
                        }
                    }
                }
            }
        }
    }
}