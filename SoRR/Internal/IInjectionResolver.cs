using System;
using System.Reflection;

namespace SoRR
{
    public interface IInjectionResolver
    {
        void Resolve(StaticInjectionInfo info, object client, object? service);
    }
    public sealed class FieldInjectionResolver(FieldInfo field) : IInjectionResolver
    {
        public FieldInfo Field { get; } = field;
        public void Resolve(StaticInjectionInfo info, object client, object? service)
            => Field.SetValue(client, service);
    }
    public sealed class PropertyInjectionResolver : IInjectionResolver
    {
        public PropertyInfo Property { get; }
        public PropertyInjectionResolver(PropertyInfo property)
        {
            if (!property.CanWrite) throw new ArgumentException("The specified property does not have a setter.");
            Property = property;
        }
        public void Resolve(StaticInjectionInfo info, object client, object? service)
            => Property.SetValue(client, service);
    }
}
