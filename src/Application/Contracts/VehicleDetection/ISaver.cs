namespace Application.Contracts.VehicleDetection;

public interface ISaver
{
    void Save(int id, Rect box, Mat frame, int frameCount);
}