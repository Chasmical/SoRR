using System;

namespace SoRR
{
    public readonly struct InjectionToken : IEquatable<InjectionToken>
    {
        public object? Value { get; }

        public InjectionToken(Type typeToken)
            => Value = typeToken;

        public static implicit operator InjectionToken(Type typeToken)
            => new InjectionToken(typeToken);

        public bool Equals(InjectionToken other)
            => Equals(Value, other.Value);
        public override bool Equals(object? obj)
            => obj is InjectionToken other && Equals(other);
        public override int GetHashCode()
            => Value?.GetHashCode() ?? 0;
        public override string ToString()
            => Value?.ToString() ?? "<null>";

        public static bool operator ==(InjectionToken left, InjectionToken right)
            => left.Equals(right);
        public static bool operator !=(InjectionToken left, InjectionToken right)
            => !left.Equals(right);

    }
}
