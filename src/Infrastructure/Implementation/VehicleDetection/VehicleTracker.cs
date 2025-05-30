using Domain.Constants;
using Domain.Models;

namespace Infrastructure.Implementation.VehicleDetection;

public class VehicleTracker : ITracker
{
    private readonly List<Vehicle> _tracked = new();
    private int _nextId = 0;
    private const double IoUThreshold = 0.6;
    private const int MaxSaves = 1;
    private const int ForgetAfterFrames = 5;

    public void Update(List<Rect> detections, Mat frame, int frameCount)
    {
        var newTracked = new List<Vehicle>();
        int nextTemporaryId = _nextId;

        foreach (var det in detections)
        {
            bool matched = false;

            // Попытка найти совпадение с уже отслеживаемыми
            foreach (var tv in _tracked)
            {
                double iou = CalculateIoU(det, tv.BoundingBox);
                if (iou > IoUThreshold)
                {
                    // Обновляем данные
                    tv.BoundingBox = det;
                    tv.FrameCount = frameCount;

                    if (tv.SaveCount < MaxSaves)
                    {
                        SaveVehicleFrame(tv.Id, det, frame, frameCount);
                        tv.SaveCount++;
                    }

                    newTracked.Add(tv);
                    matched = true;
                    break;
                }
            }

            // Если не совпало — новая машина
            if (!matched)
            {
                var veh = new Vehicle
                {
                    Id = nextTemporaryId++,
                    BoundingBox = det,
                    FrameCount = frameCount,
                    SaveCount = 1
                };
                SaveVehicleFrame(veh.Id, det, frame, frameCount);
                newTracked.Add(veh);
            }
        }

        // Удаляем те, что «пропали» больше 5 кадров
        _tracked.Clear();
        foreach (var tv in newTracked)
        {
            if (frameCount - tv.FrameCount < ForgetAfterFrames)
                _tracked.Add(tv);
        }

        _nextId = nextTemporaryId;
    }

    public void Cleanup(int currentFrame)
    {
        // Здесь можно добавить дополнительную логику очистки, если нужно
    }

    private void SaveVehicleFrame(int id, Rect bbox, Mat frame, int frameCount)
    {
        using var roi = new Mat(frame, bbox);
        string filename = Path.Combine(
            Program.OutputFolder,
            $"vehicle_{DateTime.Now:yyyyMMdd_HHmmssfff}_{frameCount}_{id}.png"
        );
        roi.SaveImage(filename);
        Console.WriteLine($"✅ Saved: {Path.GetFileName(filename)}");
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