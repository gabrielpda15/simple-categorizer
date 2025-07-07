using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleCategorizer.Common.Attributes;

namespace SimpleCategorizer.Common.Services
{
    public sealed class FormApplication
    {
        private IServiceProvider Provider { get; }

        private Action<IServiceProvider>? PreLaunchAction { get; set; }

        private FormApplication(IServiceProvider provider)
        {
            Provider = provider;
        }

        public static FormApplication Create(Action<IServiceCollection> serviceConfiguration)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            serviceConfiguration(services);

            return new FormApplication(services.BuildServiceProvider());
        }

        public FormApplication SetPreLaunchAction(Action<IServiceProvider> action)
        {
            PreLaunchAction = action;
            return this;
        }

        public void Start()
        {
            var scope = Provider.CreateScope();
            var mainForm = scope.ServiceProvider.GetRequiredKeyedService<Form>(typeof(EntryPointAttribute));

            PreLaunchAction?.Invoke(Provider);

            Application.Run(mainForm);
        }
    }
}
