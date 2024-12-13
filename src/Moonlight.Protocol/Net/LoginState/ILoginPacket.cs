namespace Moonlight.Protocol.Net.LoginState
{
    public interface ILoginPacket<T> : IPacket<T> where T : ILoginPacket<T>;
}
