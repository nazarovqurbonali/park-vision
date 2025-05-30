namespace Infrastructure.Implementations.VehicleDetection;

public sealed class VehicleTracker(ISaver saver) : ITracker
{
    private readonly List<(int Id, Rect BoundingBox, int FrameCount, int SaveCount)> _tracked = [];
    private int _nextId ;
    private const double IoUThreshold = 0.6;
    private const int MaxSaves = 1;
    private const int ForgetAfterFrames = 5;
    private readonly ISaver _saver = saver ?? throw new ArgumentNullException(nameof(saver));

    public BaseResult Update(List<Rect> detections, Mat frame, int frameCount)
    {
        try
        {
            var newTracked = new List<(int Id, Rect BoundingBox, int FrameCount, int SaveCount)>();
            int nextTemporaryId = _nextId;

            foreach (var det in detections)
            {
                bool matched = false;

                foreach (var tv in _tracked)
                {
                    double iou = CalculateIoU(det, tv.BoundingBox);
                    if (iou > IoUThreshold)
                    {
                        var updatedVehicle = (tv.Id, det, frameCount, tv.SaveCount);
                        if (updatedVehicle.SaveCount < MaxSaves)
                        {
                            var saveResult = _saver.Save(updatedVehicle.Id, det, frame, frameCount);
                            if (!saveResult.IsSuccess)
                                return BaseResult.Failure(saveResult.Error);
                            updatedVehicle.SaveCount++;
                        }

                        newTracked.Add(updatedVehicle);
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    int id = nextTemporaryId++;
                    var saveResult = _saver.Save(id, det, frame, frameCount);
                    if (!saveResult.IsSuccess)
                        return BaseResult.Failure(saveResult.Error);
                    newTracked.Add((id, det, frameCount, 1));
                }
            }

            _tracked.Clear();
            _tracked.AddRange(newTracked.Where(tv => frameCount - tv.FrameCount < ForgetAfterFrames));
            _nextId = nextTemporaryId;
            return BaseResult.Success("Tracker updated successfully.");
        }
        catch (Exception ex)
        {
            return BaseResult.Failure(ResultPatternError.InternalServerError($"Tracker update error: {ex.Message}"));
        }
    }

    public BaseResult Cleanup(int currentFrame)
    {
        try
        {
            _tracked.RemoveAll(v => currentFrame - v.FrameCount >= ForgetAfterFrames);
            return BaseResult.Success("Cleanup completed successfully.");
        }
        catch (Exception ex)
        {
            return BaseResult.Failure(ResultPatternError.InternalServerError($"Cleanup error: {ex.Message}"));
        }
    }

    private double CalculateIoU(Rect a, Rect b)
    {
        int x1 = Math.Max(a.X, b.X);
        int y1 = Math.Max(a.Y, b.Y);
        int x2 = Math.Min(a.X + a.Width, b.X + b.Width);
        int y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);

        int interArea = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
        int unionArea = a.Width * a.Height + b.Width * b.Height - interArea;
        return unionArea > 0 ? (double)interArea / unionArea : 0;
    }
}