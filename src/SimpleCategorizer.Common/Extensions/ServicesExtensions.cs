using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleCategorizer.Common.Attributes;
using System.Reflection;

namespace SimpleCategorizer.Common.Extensions
{
    public static class ServicesExtensions
    {
        [Flags]
        private enum InjectionType 
        { 
            Default = 0,
            Forced = 1 << 0,
            Keyed = 1 << 1,
            ForcedKeyed = Forced | Keyed
        }

        private static void AddDynamicService(this IServiceCollection services, ServiceLifetime lifetime, Type serviceType, object? serviceKey, Type? implementationType, bool forceInjection)
        {
            var injectionType = InjectionType.Default;
            if (forceInjection) injectionType |= InjectionType.Forced;
            if (serviceKey is not null) injectionType |= InjectionType.Keyed;

            var methodName = (lifetime, injectionType) switch
            {
                (ServiceLifetime.Singleton, InjectionType.Default) => nameof(ServiceCollectionDescriptorExtensions.TryAddSingleton),
                (ServiceLifetime.Singleton, InjectionType.Forced) => nameof(ServiceCollectionServiceExtensions.AddSingleton),
                (ServiceLifetime.Singleton, InjectionType.Keyed) => nameof(ServiceCollectionDescriptorExtensions.TryAddKeyedSingleton),
                (ServiceLifetime.Singleton, InjectionType.ForcedKeyed) => nameof(ServiceCollectionServiceExtensions.AddKeyedSingleton),
                (ServiceLifetime.Scoped, InjectionType.Default) => nameof(ServiceCollectionDescriptorExtensions.TryAddScoped),
                (ServiceLifetime.Scoped, InjectionType.Forced) => nameof(ServiceCollectionServiceExtensions.AddScoped),
                (ServiceLifetime.Scoped, InjectionType.Keyed) => nameof(ServiceCollectionDescriptorExtensions.TryAddKeyedScoped),
                (ServiceLifetime.Scoped, InjectionType.ForcedKeyed) => nameof(ServiceCollectionServiceExtensions.AddKeyedScoped),
                (ServiceLifetime.Transient, InjectionType.Default) => nameof(ServiceCollectionDescriptorExtensions.TryAddTransient),
                (ServiceLifetime.Transient, InjectionType.Forced) => nameof(ServiceCollectionServiceExtensions.AddTransient),
                (ServiceLifetime.Transient, InjectionType.Keyed) => nameof(ServiceCollectionDescriptorExtensions.TryAddKeyedTransient),
                (ServiceLifetime.Transient, InjectionType.ForcedKeyed) => nameof(ServiceCollectionServiceExtensions.AddKeyedTransient),
                _ => throw new InvalidOperationException("Cannot find injection method for selected service lifetime.")
            };

            var paramsType = new List<Type> { typeof(IServiceCollection), typeof(Type) };
            var methodParams = new List<object> { services, serviceType };

            if (serviceKey is not null)
            {
                paramsType.Add(typeof(object));
                methodParams.Add(serviceKey);
            }

            if (implementationType is not null)
            {
                paramsType.Add(typeof(Type));
                methodParams.Add(implementationType);
            }

            var extensionType = forceInjection ? typeof(ServiceCollectionServiceExtensions) : typeof(ServiceCollectionDescriptorExtensions);
            var method = extensionType.GetMethod(methodName, 0, BindingFlags.Public | BindingFlags.Static, null, [..paramsType], null);
            method?.Invoke(null, [..methodParams]);
        }

        private class Test { }

        public static IServiceCollection AddInjectables(this IServiceCollection services)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = assemblies.SelectMany(assemblies => assemblies.GetTypes())
                .Select(type => (Type: type, Attribute: type.GetCustomAttribute<InjectableAttribute>()!))
                .Where(type => type.Attribute is not null);

            foreach (var (type, attribute) in types)
            {
                var interfaces = type.GetInterfaces().Where(@interface => !type.BaseType?.GetInterfaces().Contains(@interface) ?? true);
                var serviceBaseType = attribute.IgnoreBaseType || type.BaseType == typeof(object) || type.BaseType is null ? null : type.BaseType;

                if (!interfaces.Any())
                {
                    if (serviceBaseType is null)
                    {
                        services.AddDynamicService(attribute.Lifetime, type, attribute.Key, null, attribute.Force);
                        continue;
                    }

                    services.AddDynamicService(attribute.Lifetime, serviceBaseType, attribute.Key, type, attribute.Force);
                }

                foreach (var @interface in interfaces)
                {
                    services.AddDynamicService(attribute.Lifetime, @interface, attribute.Key, type, attribute.Force);
                }
            }

            return services;
        }
    }
}
