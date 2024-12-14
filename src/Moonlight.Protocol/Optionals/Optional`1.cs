using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Moonlight.Protocol.Optionals
{
    [JsonConverter(typeof(OptionalJsonConverterFactory))]
    public readonly struct Optional<T> : IOptional
    {
        /// <summary>
        /// An <see cref="Optional{T}"/> without a value.
        /// </summary>
        public static readonly Optional<T> Empty;

        /// <summary>
        /// If the <see cref="Optional{T}"/> has a value. The value may be null.
        /// </summary>
        public bool HasValue { get; init; }

        /// <inheritdoc />
        object? IOptional.Value => _value;

        /// <summary>
        /// The value to be returned if the <see cref="Optional{T}"/> has a value.
        /// </summary>
        /// <exception cref="InvalidOperationException">If this <see cref="Optional{T}"/> has no value.</exception>
        public T? Value => HasValue ? _value : throw new InvalidOperationException("Optional<T> has no value.");

        /// <summary>
        /// The internal value. If no value is provided, this will be initialized to the default value of <typeparamref name="T"/>.
        /// </summary>
        private readonly T? _value = default!;

        /// <summary>
        /// Creates an empty instance of <see cref="Optional{T}"/>.
        /// </summary>
        public Optional() => HasValue = false;

        /// <summary>
        /// Creates an instance of <see cref="Optional{T}"/> with the specified value.
        /// </summary>
        public Optional(T? value)
        {
            _value = value;
            HasValue = true;
        }

        /// <summary>
        /// Checks if the property has a value that isn't null.
        /// </summary>
        /// <returns>If the property has a value that isn't null.</returns>
        [MemberNotNullWhen(true, nameof(Value), nameof(_value))]
        public bool IsDefined() => HasValue && _value is not null;

        /// <inheritdoc cref="IsDefined()" />
        /// <param name="value">The value of the property.</param>
        [MemberNotNullWhen(true, nameof(Value), nameof(_value))]
        public bool IsDefined([NotNullWhen(true)] out T? value) => (value = _value) is not null;

        public bool Equals(Optional<T> other) => HasValue == other.HasValue && EqualityComparer<T>.Default.Equals(_value, other._value);

        /// <inheritdoc />
        public override string ToString() => HasValue ? (_value?.ToString() ?? "null") : "<Empty>";

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Optional<T> optional && Equals(optional);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(HasValue, _value);

        public static implicit operator Optional<T>(T? value) => new(value);
        public static explicit operator T?(Optional<T> optional) => optional.Value;
        public static bool operator ==(Optional<T> optional, T value) => optional.HasValue && Equals(optional.Value, value);
        public static bool operator !=(Optional<T> optional, T value) => !optional.HasValue || !Equals(optional.Value, value);
        public static bool operator ==(Optional<T> left, Optional<T> right) => left.HasValue == right.HasValue && Equals(left.Value, right.Value);
        public static bool operator !=(Optional<T> left, Optional<T> right) => left.HasValue != right.HasValue || !Equals(left.Value, right.Value);
    }
}
