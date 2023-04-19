using System;
using System.Text.Json.Serialization;

namespace Moonlight.Protocol.Components.Chat
{
    public record ChatClickEventComponent
    {
        [JsonPropertyName("url")]
        public string Url { get; init; }

        [JsonPropertyName("run_command")]
        public string RunCommand { get; init; }

        [JsonPropertyName("suggest_command")]
        public string SuggestCommand { get; init; }

        [JsonPropertyName("change_page")]
        public int ChangePage { get; init; }

        [JsonPropertyName("copy_to_clipboard")]
        public string CopyToClipboard { get; init; }

        public ChatClickEventComponent(string url = "", string runCommand = "", string suggestCommand = "", int changePage = 1, string copyToClipboard = "")
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            RunCommand = runCommand ?? throw new ArgumentNullException(nameof(runCommand));
            SuggestCommand = suggestCommand ?? throw new ArgumentNullException(nameof(suggestCommand));
            ChangePage = changePage;
            CopyToClipboard = copyToClipboard ?? throw new ArgumentNullException(nameof(copyToClipboard));
        }
    }
}
