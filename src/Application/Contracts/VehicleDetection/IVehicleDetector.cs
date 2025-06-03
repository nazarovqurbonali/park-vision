namespace Application.Contracts.VehicleDetection;

public interface IVehicleDetector
{
    Result<List<Rect>> DetectVehicles(Mat frame);
}