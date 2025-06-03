namespace Application.Contracts.VehicleDetection;

public interface IImageSaver
{
    BaseResult SaveImage(Mat image, string filePath);
}