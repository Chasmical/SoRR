using System;

namespace SoRR
{
    /// <summary>
    ///   <para>Specifies that an input/output may be Unity's fake null.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public sealed class MaybeFakeNullAttribute : Attribute;
}
