namespace Infrastructure.Implementations.VehicleDetection;

public sealed class VideoProcessor(
    ILogger<VideoProcessor> logger) : IVideoProcessor
{
    public BaseResult ProcessVideo(string videoPath, Action<Mat, int> frameProcessor)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(ProcessVideo), date);

        using VideoCapture capture = new(videoPath);
        if (!capture.IsOpened())
        {   
            logger.LogCritical(Messages.VideoProcessorNotFoundVideo);
            logger.OperationCompleted(nameof(ProcessVideo), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            return BaseResult.Failure(ResultPatternError.NotFound(Messages.VideoProcessorNotFoundVideo));
        }

        int frameCount = 0;
        while (true)
        {
            using Mat frame = new();
            if (!capture.Read(frame) || frame.Empty())
                break;

            frameProcessor(frame, frameCount);
            frameCount++;
        }

        logger.OperationCompleted(nameof(ProcessVideo), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
        return BaseResult.Success();
    }

    public BaseResult CheckCameraAvailability(string videoPath)
    {
        using VideoCapture capture = new(videoPath);
        if (!capture.IsOpened())
            return BaseResult.Failure(ResultPatternError.NotFound(Messages.VideoProcessorNotFoundVideo));

        return BaseResult.Success();
    }
}