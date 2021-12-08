using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Moonlight.Api.Networking.Packets
{
    [SuppressMessage("Roslyn", "CS8618", Justification = "Empty constructors are required for object initialization.")]
    public class Packet
    {
        [JsonIgnore]
        public virtual int Id { get; set; }

        [JsonIgnore]
        public byte[]? Data { get; set; }

        // TODO: Consider generating constructors on inherited types to prevent having to copy and paste this throughout other Packet classes.
        public Packet() { }

        public Packet(int id = 0, params byte[] data)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            ArgumentNullException.ThrowIfNull(data, nameof(data));

            Id = id;
            Data = data;
        }

        public virtual void UpdateProperties() => throw new NotImplementedException();
        public virtual void UpdateData() => throw new NotImplementedException();
        public virtual int CalculatePacketLength() => Id.GetVarLength() + Data?.Length ?? 1;
        public override bool Equals(object? obj) => obj is Packet packet && Id == packet.Id && EqualityComparer<byte[]?>.Default.Equals(Data, packet.Data);
        public override int GetHashCode() => HashCode.Combine(Id, Data);
        public static bool operator ==(Packet packet1, Packet packet2) => (packet1.Id == 0 && packet1.Data == null && packet2.Id == 0 && packet2.Data == null) || packet1.Equals(packet2);
        public static bool operator !=(Packet packet1, Packet packet2) => !(packet1 == packet2);
    }
}