namespace WebUI.Extensions.DI;

public static class RegisterService
{
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllersWithViews();
        
        builder.Services.AddSingleton<IModelLoader, ModelLoader>();

        builder.Services.AddSingleton<IImageSaver, ImageSaver>();

        builder.Services.AddSingleton<IVehicleDetectionService, VehicleDetectionService>();

        builder.Services.AddSingleton<IVehicleTracker, VehicleTracker>(x =>
            new(
                x.GetRequiredService<IImageSaver>(),
                builder.Configuration.GetRequiredString(ConfigNames.OutputFolder),
                builder.Configuration.GetRequiredInt(ConfigNames.MaxSavesPerVehicle),
                x.GetRequiredService<ILogger<VehicleTracker>>())
        );


        builder.Services.AddSingleton<IVideoProcessor, VideoProcessor>();
        builder.Services.AddSingleton<ILiveCameraService, LiveCameraService>();
        return builder;
    }
}