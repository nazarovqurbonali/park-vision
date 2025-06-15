using System.Windows;
using WpfApp.Views;

namespace WpfApp;

public sealed partial class App : System.Windows.Application
{
    private readonly IHost _host;

    public App()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(
                ConfigNames.AppSettingJson,
                optional: false,
                reloadOnChange: true)
            .Build();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<App>();
                services.AddSingleton<MainWindow>();
                services.AddSingleton<IConfiguration>(configuration);

                services.AddSingleton<IModelLoader, ModelLoader>();

                services.AddSingleton<IImageSaver, ImageSaver>();

                services.AddSingleton<IVehicleDetectionService, VehicleDetectionService>();

                services.AddSingleton<IVehicleTracker, VehicleTracker>(x =>
                    new(
                        x.GetRequiredService<IImageSaver>(),
                        configuration.GetRequiredString(ConfigNames.OutputFolder),
                        configuration.GetRequiredInt(ConfigNames.MaxSavesPerVehicle),
                        x.GetRequiredService<ILogger<VehicleTracker>>())
                );


                services.AddSingleton<IVideoProcessor, VideoProcessor>();
            }).Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        MainWindow mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }
}