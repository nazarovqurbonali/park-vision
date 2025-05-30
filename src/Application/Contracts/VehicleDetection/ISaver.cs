namespace Application.Contracts.VehicleDetection;

public interface ISaver
{
    BaseResult Save(int id, Rect box, Mat frame, int frameCount);
}