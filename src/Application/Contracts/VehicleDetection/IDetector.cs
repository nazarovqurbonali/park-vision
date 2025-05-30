namespace Application.Contracts.VehicleDetection;

public interface IDetector
{
    Result<List<Rect>> Detect(Mat frame);
}