using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PetOffice.Core.Persistence;
using PetOffice.WpfApp.Services;
using PetOffice.WpfApp.Services.Storage;
using PetOffice.WpfApp.ViewModels;

namespace PetOffice.WpfApp
{
    public partial class App : Application
    {
        private ServiceProvider? serviceProvider;

        protected override void OnStartup(
            StartupEventArgs e)
        {
            base.OnStartup(e);

            ServiceCollection services =
                new();

            ConfigureServices(services);

            serviceProvider =
                services.BuildServiceProvider();

            MainWindow =
                serviceProvider
                    .GetRequiredService<MainWindow>();

            MainWindow.Show();
        }

        private static void ConfigureServices(
            IServiceCollection services)
        {
            services.AddKeyedSingleton<
                IWorldStorage,
                JsonWorldStorage>("json");

            services.AddKeyedSingleton<
                IWorldStorage,
                XmlWorldStorage>("xml");

            services.AddKeyedSingleton<
    IWorldStorage,
    LocalDbWorldStorage>("localdb");

            services.AddSingleton<
                IFileDialogService,
                WpfFileDialogService>();

            services.AddTransient<MainViewModel>();

            services.AddTransient<MainWindow>();
        }

        protected override void OnExit(
            ExitEventArgs e)
        {
            serviceProvider?.Dispose();

            base.OnExit(e);
        }
    }
}