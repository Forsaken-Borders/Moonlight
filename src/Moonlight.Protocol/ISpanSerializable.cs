using System;

namespace Moonlight.Protocol
{
    public interface ISpanSerializable<T> where T : ISpanSerializable<T>
    {
        public int Serialize(Span<byte> target);
        public static abstract T Deserialize(ReadOnlySpan<byte> data, out int offset);
    }
}
