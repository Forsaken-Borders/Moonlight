using System;

namespace Moonlight.Types
{
    public class MojangSessionServerResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public object[] Properties { get; init; }
    }
}