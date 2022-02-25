using Moonlight.Api.Server.Enums;

namespace Moonlight.Api.Server.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class SubscribeToEventAttribute : Attribute
    {
        public EventName EventName { get; init; }
        public EventPriority Priority { get; init; }

        public SubscribeToEventAttribute(EventName eventName, EventPriority priority = EventPriority.Normal)
        {
            EventName = eventName;
            Priority = priority;
        }
    }
}