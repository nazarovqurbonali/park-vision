namespace Infrastructure.Implementations.VehicleDetection;

public sealed class VehicleTracker(
    IImageSaver imageSaver,
    string outputFolder,
    int maxSavesPerVehicle,
    ILogger<VehicleTracker> logger) : IVehicleTracker
{
    private readonly List<Vehicle> _trackedVehicles = [];
    private int _nextVehicleId;
    private const double IoUThreshold = 0.6;
    private const int MaxFramesToKeepVehicle = 5;

    public void UpdateTrackedVehicles(List<Rect> newRects, Mat frame, int frameCount)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(UpdateTrackedVehicles), date);

        List<Rect> unmatchedRects = newRects;

        List<Vehicle> newTrackedVehicles = [];
        int vehicleId = _nextVehicleId;

        foreach (Rect newRect in newRects)
        {
            bool matched = false;
            foreach (Vehicle tracked in _trackedVehicles)
            {
                double iou = CalculateIoU(newRect, tracked.BoundingBox);
                if (iou > IoUThreshold)
                {
                    tracked.BoundingBox = newRect;
                    tracked.FrameCount = frameCount;
                    if (tracked.SaveCount < maxSavesPerVehicle)
                    {
                        BaseResult result = SaveVehicleImage(frame, newRect, frameCount, tracked.Id);
                        if (result.IsSuccess)
                            tracked.SaveCount++;
                        else
                            logger.LogError(result.Error.Message);
                    }

                    newTrackedVehicles.Add(tracked);
                    unmatchedRects.Remove(newRect);
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                BaseResult result = SaveVehicleImage(frame, newRect, frameCount, vehicleId);
                if (result.IsSuccess)
                {
                    newTrackedVehicles.Add(new()
                    {
                        Id = vehicleId++,
                        BoundingBox = newRect,
                        FrameCount = frameCount,
                        SaveCount = 1
                    });
                }
                else
                    logger.LogError(result.Error.Message);
            }
        }

        _trackedVehicles.Clear();
        _trackedVehicles.AddRange(newTrackedVehicles.Where(v => frameCount - v.FrameCount < MaxFramesToKeepVehicle));
        _nextVehicleId = vehicleId;

        logger.OperationCompleted(nameof(UpdateTrackedVehicles), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
    }

    private BaseResult SaveVehicleImage(Mat frame, Rect rect, int frameCount, int vehicleId)
    {
        using Mat vehicleRoi = new(frame, rect);
        string timestamp = $"{DateTime.Now:yyyyMMdd_HHmmssfff}_{frameCount}_{vehicleId}";
        string vehiclePath = Path.Combine(outputFolder, $"vehicle_{timestamp}.png");

        BaseResult result = imageSaver.SaveImage(vehicleRoi, vehiclePath);
        if (result.IsSuccess)
            logger.LogInformation($"Vehicle image saved: {vehiclePath}");
        return result;
    }

    private double CalculateIoU(Rect rect1, Rect rect2)
    {
        int x1 = Math.Max(rect1.X, rect2.X);
        int y1 = Math.Max(rect1.Y, rect2.Y);
        int x2 = Math.Min(rect1.X + rect1.Width, rect2.X + rect2.Width);
        int y2 = Math.Min(rect1.Y + rect1.Height, rect2.Y + rect2.Height);

        int intersectionArea = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
        int unionArea = rect1.Width * rect1.Height + rect2.Width * rect2.Height - intersectionArea;
        return unionArea > 0 ? (double)intersectionArea / unionArea : 0;
    }
}