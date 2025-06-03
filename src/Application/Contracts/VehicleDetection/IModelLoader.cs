namespace Application.Contracts.VehicleDetection;

public interface IModelLoader
{
    Result<Net> LoadModel(string configPath, string weightsPath);
    Result<string[]> LoadClassNames(string namesPath);
}