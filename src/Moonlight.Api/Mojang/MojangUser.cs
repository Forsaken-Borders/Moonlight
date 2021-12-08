namespace Moonlight.Api.Mojang
{
    public class MojangUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Legacy { get; set; }
        public bool Demo { get; set; }
        public List<MojangUserSkinProperty> Properties { get; set; }

        public MojangUser(string id, string name, bool legacy, bool demo, params MojangUserSkinProperty[] properties)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id), "String cannot be null or empty.");
            }
            else if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name), "String cannot be null or empty.");
            }
            ArgumentNullException.ThrowIfNull(properties);

            Id = id;
            Name = name;
            Legacy = legacy;
            Demo = demo;
            Properties = new(properties);
        }

        public override bool Equals(object? obj) => obj is MojangUser user && Id == user.Id && Name == user.Name && Legacy == user.Legacy && Demo == user.Demo && EqualityComparer<List<MojangUserSkinProperty>>.Default.Equals(Properties, user.Properties);
        public override int GetHashCode() => HashCode.Combine(Id, Name, Legacy, Demo, Properties);
    }
}