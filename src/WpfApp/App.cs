using System.Windows;
using Application.Extensions.ResultPattern;

namespace WpfApp;

public class App(
    MainWindow mainWindow,
    IVehicleDetectionService vehicleDetectionService,
    IConfiguration configuration)
    : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {

        mainWindow.Show();
        base.OnStartup(e);
    }

    //for testing
    private void TestVehicleDetectionService()
    {
        try
        {
            string weightsPath = configuration.GetRequiredString(ConfigNames.WeightsPath);
            string configPath = configuration.GetRequiredString(ConfigNames.ConfigPath);
            string namesPath = configuration.GetRequiredString(ConfigNames.NamesPath);
            string videoPath = configuration.GetRequiredString(ConfigNames.VideoPath);
            string outputFolder = configuration.GetRequiredString(ConfigNames.OutputFolder);
            int maxSavesPerVehicle = configuration.GetRequiredInt(ConfigNames.MaxSavesPerVehicle);

            BaseResult result = vehicleDetectionService.Run(
                configPath,
                weightsPath,
                namesPath,
                videoPath,
                outputFolder,
                maxSavesPerVehicle
            );

            if (result.IsSuccess)
            {
                MessageBox.Show("Vehicle detection completed successfully! Check the output folder.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Vehicle detection failed: {result.Error.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
