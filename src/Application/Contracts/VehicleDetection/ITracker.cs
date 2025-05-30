namespace Application.Contracts.VehicleDetection;

public interface ITracker
{
    BaseResult Update(List<Rect> detections, Mat frame, int frameCount);
    BaseResult Cleanup(int currentFrame);
}