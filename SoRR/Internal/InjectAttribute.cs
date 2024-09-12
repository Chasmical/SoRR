using System;
using JetBrains.Annotations;

namespace SoRR
{
    [MeansImplicitUse, AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class InjectAttribute : Attribute
    {
        public string? Path { get; }

        public InjectAttribute() { }
        public InjectAttribute(string path) => Path = path;

    }
}
