namespace Application.Contracts.VehicleDetection;

public interface IVideoSource
{
    bool Open();
    bool ReadFrame(out Mat frame);
    void Release();
}