using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Moonlight.Protocol
{
    public interface ISpanSerializable<T> where T : ISpanSerializable<T>
    {
        public int Serialize(Span<byte> target);
        public static abstract T Deserialize(ref SequenceReader<byte> reader);
        public static abstract bool TryDeserialize(ref SequenceReader<byte> reader, [NotNullWhen(true)] out T? result);
    }
}
