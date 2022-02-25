using Moonlight.Api.Server.Enums;
using System.Reflection;

namespace Moonlight.Api.Server.Abstractions
{
    /// <summary>
    /// Represents a method that will handle an event.
    /// </summary>
    public interface IEventSubscription
    {
        /// <summary>
        /// The event name.
        /// </summary>
        EventName EventName { get; }

        /// <summary>
        /// The event handler.
        /// </summary>
        MethodInfo EventHandler { get; }

        /// <summary>
        /// Defines the event's priority. Events are executed in ascending order of priority.
        /// </summary>
        EventPriority Priority { get; }

        /// <summary>
        /// The plugin that contains the event handler.
        /// </summary>
        Plugin Plugin { get; }
    }
}