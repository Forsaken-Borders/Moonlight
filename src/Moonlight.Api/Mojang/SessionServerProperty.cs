namespace Moonlight.Api.Mojang
{
    public class SessionServerProperty
    {
        public string Name { get; init; }
        public string Value { get; init; }
        public string Signature { get; init; }

        public SessionServerProperty(string name, string value, string signature)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name), "String cannot be null or empty.");
            }
            else if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value), "String cannot be null or empty.");
            }
            else if (string.IsNullOrWhiteSpace(signature))
            {
                throw new ArgumentNullException(nameof(signature), "String cannot be null or empty.");
            }

            Name = name;
            Value = value;
            Signature = signature;
        }

        public override bool Equals(object? obj) => obj is SessionServerProperty property && Name == property.Name && Value == property.Value && Signature == property.Signature;
        public override int GetHashCode() => HashCode.Combine(Name, Value, Signature);
    }
}