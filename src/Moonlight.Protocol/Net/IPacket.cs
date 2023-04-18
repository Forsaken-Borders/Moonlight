using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Protocol.Net
{
    public interface IPacket<T> : ISpanSerializable<T> where T : IPacket<T>
    {
        /// <summary>
        /// The id of the packet.
        /// </summary>
        public static abstract VarInt Id { get; }

        /// <summary>
        /// Calculates the size of the packet in bytes.
        /// </summary>
        /// <returns>The size of the packet in bytes.</returns>
        public int CalculateSize();
    }
}
