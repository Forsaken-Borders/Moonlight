using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Moonlight.Types
{
    public class MojangSessionServerResponse
    {
        [JsonIgnore, SuppressMessage("Roslyn", "IDE1006", Justification = "This private property is used to avoid issues with JSON Deserialization.")]
        private Guid _id { get; init; }

        public string Id { get => _id.ToString(); init => _id = Guid.Parse(value); }
        public string Name { get; init; }
        public List<JoinedProperty> Properties { get; init; } = new();

        public override bool Equals(object obj) => obj is MojangSessionServerResponse response && _id.Equals(response._id) && Id == response.Id && Name == response.Name && EqualityComparer<List<JoinedProperty>>.Default.Equals(Properties, response.Properties);
        public override int GetHashCode() => HashCode.Combine(Id, Name, Properties);
    }

    public class JoinedProperty
    {
        public string Name { get; init; }
        public string Value { get; init; }
        public string Signature { get; init; }

        public override bool Equals(object obj) => obj is JoinedProperty property && Name == property.Name && Value == property.Value && Signature == property.Signature;
        public override int GetHashCode() => HashCode.Combine(Name, Value, Signature);
    }
}