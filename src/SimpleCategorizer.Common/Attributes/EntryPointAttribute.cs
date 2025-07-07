using Microsoft.Extensions.DependencyInjection;

namespace SimpleCategorizer.Common.Attributes
{
    public sealed class EntryPointAttribute : InjectableAttribute
    {
        public EntryPointAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped) : base(lifetime)
        {
            Key = typeof(EntryPointAttribute);
            Force = true;
        }
    }
}
