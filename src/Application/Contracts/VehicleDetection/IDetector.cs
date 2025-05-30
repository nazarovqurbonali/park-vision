namespace Application.Contracts.VehicleDetection;

public interface IDetector
{
    List<Rect> Detect(Mat frame);
}