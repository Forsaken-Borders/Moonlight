using System.Text.Json.Serialization;

namespace Moonlight.Api.Minecraft.Abstractions.Networking.Packets
{
    public abstract record AbstractPacket
    {
        /// <summary>
        /// The packet ID.
        /// </summary>
        [JsonIgnore]
        public virtual int Id { get; internal set; }

        /// <summary>
        /// The packet data. It's recommended that you use the properties of the class instead of accessing the data directly.
        /// </summary>
        [JsonIgnore]
        public virtual byte[]? Data { get; internal set; }

        public AbstractPacket() { }

        public AbstractPacket(int id, params byte[] data)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            ArgumentNullException.ThrowIfNull(data, nameof(data));

            Id = id;
            Data = data;
            UpdateProperties();
        }

        /// <summary>
        /// Reads from the <see cref="Data"/> property, and updates the rest of the class properties.
        /// </summary>
        public abstract void UpdateProperties();

        /// <summary>
        /// Reads from the class properties, and updates the <see cref="Data"/> property.
        /// </summary>
        public abstract void UpdateData();

        /// <summary>
        /// Calculates the length of the packet.
        /// </summary>
        /// <returns>The length of the packet.</returns>
        public abstract int CalculatePacketLength();
    }
}