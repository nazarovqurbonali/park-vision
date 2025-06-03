namespace Application.Contracts.VehicleDetection;

public interface IVehicleDetectionService
{
    BaseResult Run(string configPath, string weightsPath, string namesPath, string videoPath, string outputFolder,int maxSavesPerVehicle);
}