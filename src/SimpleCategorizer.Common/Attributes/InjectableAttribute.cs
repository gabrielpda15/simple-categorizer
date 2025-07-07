using Microsoft.Extensions.DependencyInjection;

namespace SimpleCategorizer.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InjectableAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped) : Attribute
    {
        public ServiceLifetime Lifetime { get; } = lifetime;
        public bool Force { get; set; } = false;
        public object? Key { get; set; } = null;
        public bool IgnoreBaseType { get; set; } = false;
    }
}
