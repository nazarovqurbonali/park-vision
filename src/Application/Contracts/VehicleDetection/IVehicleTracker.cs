namespace Application.Contracts.VehicleDetection;

public interface IVehicleTracker
{
    void UpdateTrackedVehicles(List<Rect> newRects, Mat frame, int frameCount);
}