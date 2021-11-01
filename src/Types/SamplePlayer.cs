using System;
using System.Text.Json.Serialization;

namespace Moonlight.Types
{
    public class SamplePlayer
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; }

        public override bool Equals(object obj) => obj is SamplePlayer player && Id.Equals(player.Id) && Name == player.Name;
        public override int GetHashCode() => HashCode.Combine(Id, Name);
    }
}