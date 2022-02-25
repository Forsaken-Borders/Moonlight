using Moonlight.Api.Server.Abstractions;
using Moonlight.Api.Server.Enums;
using System;
using System.Reflection;

namespace Moonlight.Server.Objects
{
    public class EventSubscription : IEventSubscription
    {
        public EventName EventName { get; init; }
        public MethodInfo EventHandler { get; init; }
        public EventPriority Priority { get; init; }
        public Plugin Plugin { get; init; }

        public EventSubscription(EventName eventName, EventPriority priority, MethodInfo eventHandler, Plugin plugin)
        {
            ArgumentNullException.ThrowIfNull(eventHandler, nameof(eventHandler));
            ArgumentNullException.ThrowIfNull(plugin, nameof(plugin));

            EventName = eventName;
            Priority = priority;
            EventHandler = eventHandler;
            Plugin = plugin;
        }
    }
}