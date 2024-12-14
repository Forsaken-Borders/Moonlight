namespace Moonlight.Protocol.Optionals
{
    public interface IOptional
    {
        public bool HasValue { get; }
        public object? Value { get; }
    }
}
