using System;

namespace Moonlight.Api
{
    public interface IPacket
    {
        /// <summary>
        /// The id of the packet.
        /// </summary>
        public static abstract int Id { get; }

        /// <summary>
        /// Deserializes the packet from the given <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="data">The <see cref="Span{T}"/> to read the data from.</param>
        /// <typeparam name="TPacket">The type of the packet to return.</typeparam>
        /// <returns>The new packet.</returns>
        public static abstract TPacket Deserialize<TPacket>(Span<byte> data) where TPacket : IPacket;

        /// <summary>
        /// Serializes the packet to the given <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="target">The <see cref="Span{T}"/> to write the data to.</param>
        public abstract void Serialize(Span<byte> target);

        /// <summary>
        /// Calculates the size of the packet in bytes.
        /// </summary>
        /// <returns>The size of the packet in bytes.</returns>
        public abstract int CalculateSize();
    }
}
