namespace Application.Contracts.VehicleDetection;

public interface ISaver
{
    void Save(int id, Rect bbox, Mat frame, int frameCount);
}