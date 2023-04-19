using System;

namespace Moonlight.Protocol.Components.Chat
{
    public record ChatHoverEventComponent
    {
        public string ShowText { get; init; }
        public string ShowItem { get; init; }
        public string ShowEntity { get; init; }

        public ChatHoverEventComponent(string showText = "", string showItem = "", string showEntity = "")
        {
            ShowText = showText ?? throw new ArgumentNullException(nameof(showText));
            ShowItem = showItem ?? throw new ArgumentNullException(nameof(showItem));
            ShowEntity = showEntity ?? throw new ArgumentNullException(nameof(showEntity));
        }
    }
}
