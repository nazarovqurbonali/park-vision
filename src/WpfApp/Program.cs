using Microsoft.Extensions.Logging;

namespace WpfApp;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(
                ConfigNames.AppSettingJson,
                optional: false,
                reloadOnChange: true)
            .Build();

        IHost host = Host.CreateDefaultBuilder()
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


        App app = host.Services.GetRequiredService<App>();

        app.Run();
    }
}