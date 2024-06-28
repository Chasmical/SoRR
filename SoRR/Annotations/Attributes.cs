#if !NET5_0_OR_GREATER
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class MemberNotNullAttribute(params string[] members) : Attribute
    {
        public string[] Members { get; } = members;
        public MemberNotNullAttribute(string member) : this([member]) { }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class MemberNotNullWhenAttribute(bool returnValue, params string[] members) : Attribute
    {
        public bool ReturnValue { get; } = returnValue;
        public string[] Members { get; } = members;
        public MemberNotNullWhenAttribute(bool returnValue, string member) : this(returnValue, [member]) { }
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ModuleInitializerAttribute : Attribute;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Event |
                    AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Module |
                    AttributeTargets.Property | AttributeTargets.Struct, Inherited = false)]
    public sealed class SkipLocalsInitAttribute : Attribute;
}
#endif

#if !NET7_0_OR_GREATER
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Constructor, Inherited = false)]
    public sealed class SetsRequiredMembersAttribute : Attribute;
}
#endif
