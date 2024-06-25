using System;
using JetBrains.Annotations;

namespace SoRR
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property), MeansImplicitUse]
    public class InjectAttribute : Attribute, IEquatable<InjectAttribute>
    {
        public InjectionToken? Token { get; }
        public string? Path { get; }

        public InjectAttribute() { }
        public InjectAttribute(string path)
            => Path = path;

        public bool Equals(InjectAttribute? other)
        {
            if (other is null) return false;
            return Token == other.Token && Path == other.Path;
        }
        public override bool Equals(object? obj)
            => Equals(obj as InjectAttribute);
        public override int GetHashCode()
            => HashCode.Combine(Token, Path);

        public override string ToString()
            => $"{Path ?? "."}: {Token}";

    }
}
