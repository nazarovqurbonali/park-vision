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

                services.AddSingleton<IVideoSource, VideoSource>(
                    _ => new(configuration.GetRequiredString(ConfigNames.VideoPath)));

                services.AddSingleton<IDetector, YoloDetector>(_ =>
                    new(
                        configuration.GetRequiredString(ConfigNames.ConfigPath),
                        configuration.GetRequiredString(ConfigNames.WeightsPath),
                        configuration.GetRequiredString(ConfigNames.NamesPath)));

                services.AddSingleton<ITracker, VehicleTracker>();

                services.AddSingleton<ISaver, VehicleSaver>(
                    _ => new(configuration.GetRequiredString(ConfigNames.OutputFolder)));

                services.AddSingleton<VideoProcessor>();
            }).Build();


        App app = host.Services.GetRequiredService<App>();

        app.Run();
    }
}