using System;
using System.Collections.Generic;

namespace Moonlight.Types
{
    public class MojangSessionServerResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public object[] Properties { get; init; }

        public override bool Equals(object obj) => obj is MojangSessionServerResponse response && Id.Equals(response.Id) && Name == response.Name && EqualityComparer<object[]>.Default.Equals(Properties, response.Properties);
        public override int GetHashCode() => HashCode.Combine(Id, Name, Properties);
    }
}