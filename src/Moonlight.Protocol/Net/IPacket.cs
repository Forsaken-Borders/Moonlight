using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Protocol.Net
{
    public interface IPacket
    {
        /// <summary>
        /// The id of the packet.
        /// </summary>
        public VarInt Id { get; }
    }

    public interface IPacket<T> : IPacket, ISpanSerializable<T> where T : IPacket<T>
    {
        /// <summary>
        /// The id of the packet.
        /// </summary>
        public static new abstract VarInt Id { get; }
        VarInt IPacket.Id => T.Id;
    }
}
