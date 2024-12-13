namespace Moonlight.Protocol.Net.StatusState
{
    public interface IStatusPacket<T> : IPacket<T> where T : IStatusPacket<T>;
}
