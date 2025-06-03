namespace Infrastructure.Implementations.VehicleDetection;

public sealed class VehicleDetectionService(
    IModelLoader modelLoader,
    IVideoProcessor videoProcessor,
    IImageSaver imageSaver,
    ILoggerFactory loggerFactory,
    ILogger<VehicleDetectionService> logger) : IVehicleDetectionService
{
    private const int FrameSkip = 5;
    private const long MemoryLimitBytes = 500 * 1024 * 1024;

    public BaseResult Run(string configPath,
        string weightsPath, string namesPath, string videoPath,
        string outputFolder, int maxSavesPerVehicle)
    {
        DateTimeOffset startTime = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(Run), startTime);

        if (!Directory.Exists(outputFolder))
        {
            try
            {
                Directory.CreateDirectory(outputFolder);
            }
            catch (Exception ex)
            {
                logger.OperationException(nameof(Run), ex.Message);
                logger.OperationCompleted(nameof(Run), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - startTime);

                return BaseResult.Failure(ResultPatternError.InternalServerError(ex.Message));
            }
        }

        Result<Net> modelResult = modelLoader.LoadModel(configPath, weightsPath);
        if (!modelResult.IsSuccess)
        {
            logger.LogError(modelResult.Error.Message);
            return BaseResult.Failure(modelResult.Error);
        }

        Net net = modelResult.Value!;
        net.SetPreferableBackend(Backend.OPENCV);
        net.SetPreferableTarget(Target.CPU);

        Result<string[]> classNamesResult = modelLoader.LoadClassNames(namesPath);
        if (!classNamesResult.IsSuccess)
        {
            logger.LogError(classNamesResult.Error.Message);
            return BaseResult.Failure(classNamesResult.Error);
        }

        string[] classNames = classNamesResult.Value!;

        ILogger<VehicleDetector> detectorLogger =
            loggerFactory.CreateLogger<VehicleDetector>();
        IVehicleDetector detector = new VehicleDetector(net, classNames, detectorLogger);

        ILogger<VehicleTracker> trackerLogger = loggerFactory.CreateLogger<VehicleTracker>();
        IVehicleTracker tracker = new VehicleTracker(imageSaver, outputFolder, maxSavesPerVehicle, trackerLogger);

        void FrameProcessor(Mat frame, int frameIndex)
        {
            if (frameIndex % FrameSkip != 0)
                return;

            logger.LogInformation($"Frame processing {frameIndex}...");

            Result<List<Rect>> detectionResult = detector.DetectVehicles(frame);
            if (detectionResult.IsSuccess)
                tracker.UpdateTrackedVehicles(detectionResult.Value!, frame, frameIndex);
            else
                logger.LogError($"Detection error on frame {frameIndex}: {detectionResult.Error.Message}");

            long memoryUsed = GC.GetTotalMemory(false);
            if (memoryUsed > MemoryLimitBytes)
            {
                logger.LogInformation($"Clearing Memory: {memoryUsed / 1024 / 1024} MB");
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        BaseResult processResult = videoProcessor.ProcessVideo(videoPath, FrameProcessor);
        if (!processResult.IsSuccess)
        {
            logger.LogError($"Video processing error: {processResult.Error.Message}");
            return processResult;
        }

        logger.OperationCompleted(nameof(Run), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - startTime);
        return BaseResult.Success();
    }
}