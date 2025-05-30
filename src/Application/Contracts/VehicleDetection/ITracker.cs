namespace Application.Contracts.VehicleDetection;

public interface ITracker
{
    void Update(List<Rect> detections, Mat frame, int frameCount);
    void Cleanup(int currentFrame);
}