namespace Moonlight.Protocol.Components.Chat
{
    public record ChatClickEventComponent
    {
        public string? Url { get; init; }
        public string? RunCommand { get; init; }
        public string? SuggestCommand { get; init; }
        public int ChangePage { get; init; }
        public string? CopyToClipboard { get; init; }

        public ChatClickEventComponent() { }
    }
}
