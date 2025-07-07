using Microsoft.Extensions.DependencyInjection;
using SimpleCategorizer.Common.Extensions;
using SimpleCategorizer.Common.Services;
using SimpleCategorizer.Data.Extensions;

namespace SimpleCategorizer
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            FormApplication.Create(services =>
            {
                services.AddInjectables();
            }).Start();
        }
    }
}