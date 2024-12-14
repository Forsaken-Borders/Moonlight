using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Moonlight.Protocol
{
    public interface ISpanSerializable<T> where T : ISpanSerializable<T>
    {
        public static abstract int CalculateSize(T packet);
        public static abstract int Serialize(T packet, Span<byte> target);
        public static abstract T Deserialize(ref SequenceReader<byte> reader);
        public static abstract bool TryDeserialize(ref SequenceReader<byte> reader, [NotNullWhen(true)] out T? result);
    }
}
