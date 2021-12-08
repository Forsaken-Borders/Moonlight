using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Moonlight.Api.Mojang
{
    public class SessionServerResponse
    {
        [JsonIgnore, SuppressMessage("Roslyn", "IDE1006", Justification = "This property is private to avoid issues with JSON Deserialization.")]
        private Guid id { get; init; }

        public string Id { get => id.ToString(); init => id = Guid.Parse(value); }
        public string Name { get; init; }
        public List<SessionServerProperty> Properties { get; init; } = new();

        public SessionServerResponse(Guid id, string name, params SessionServerProperty[] properties)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name), "String cannot be null or empty.");
            }
            ArgumentNullException.ThrowIfNull(properties, nameof(properties));

            this.id = id;
            Name = name;
            Properties = new(properties);
        }

        public SessionServerResponse(string id, string name, params SessionServerProperty[] properties)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id), "String cannot be null or empty.");
            }
            else if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name), "String cannot be null or empty.");
            }
            ArgumentNullException.ThrowIfNull(properties, nameof(properties));

            Id = id;
            Name = name;
            Properties = new(properties);
        }

        public override bool Equals(object? obj) => obj is SessionServerResponse response && id.Equals(response.id) && Id == response.Id && Name == response.Name && EqualityComparer<List<SessionServerProperty>>.Default.Equals(Properties, response.Properties);
        public override int GetHashCode() => HashCode.Combine(id, Id, Name, Properties);
    }
}