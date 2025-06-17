namespace Application.Contracts.VehicleDetection;

public interface ILiveCameraService
{
    event Action<Mat> FrameReceived;
    BaseResult Start(string url);
    BaseResult Stop();
}