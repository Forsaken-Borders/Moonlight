
namespace Moonlight.Api.Server.Abstractions
{
    public interface IEventManager
    {
        IReadOnlyList<IEventSubscription> EventSubscriptions { get; }

        void Subscribe(IEventSubscription subscription);
        void Unsubscribe(IEventSubscription subscription);
    }
}