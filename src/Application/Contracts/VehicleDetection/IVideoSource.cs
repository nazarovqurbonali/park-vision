namespace Application.Contracts.VehicleDetection;

public interface IVideoSource
{
    Result<bool> Open();
    Result<bool> ReadFrame(out Mat frame);
    BaseResult Release();
}