namespace Moonlight.Protocol.Net
{
    public interface IServerPacket<T> : IPacket<T> where T : IPacket<T>;
}
