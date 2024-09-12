using System;

namespace SoRR
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public sealed class MaybeFakeNullAttribute : Attribute;
}
