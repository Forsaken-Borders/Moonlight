namespace Moonlight.Protocol.Components.Chat
{
    public record ChatHoverEventComponent
    {
        public string? ShowText { get; init; }
        public string? ShowItem { get; init; }
        public string? ShowEntity { get; init; }

        public ChatHoverEventComponent() { }
    }
}
