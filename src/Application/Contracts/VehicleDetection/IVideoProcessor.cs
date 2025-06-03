namespace Application.Contracts.VehicleDetection;

public interface IVideoProcessor
{
    BaseResult ProcessVideo(string videoPath, Action<Mat, int> frameProcessor);
}