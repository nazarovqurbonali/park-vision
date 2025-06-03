namespace Infrastructure.Implementations.VehicleDetection;

public sealed class ImageSaver(ILogger<ImageSaver> logger) : IImageSaver
{
    public BaseResult SaveImage(Mat image, string filePath)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(SaveImage), date);

        try
        {
            image.SaveImage(filePath);
            return BaseResult.Success();
        }
        catch (Exception ex)
        {
            logger.OperationException(nameof(SaveImage), ex.Message);
            logger.OperationCompleted(nameof(SaveImage), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);

            return BaseResult.Failure(ResultPatternError.InternalServerError(ex.Message));
        }
    }
}