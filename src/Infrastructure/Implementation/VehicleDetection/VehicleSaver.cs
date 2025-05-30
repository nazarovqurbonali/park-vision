namespace Infrastructure.Implementation.VehicleDetection;

public sealed class VehicleSaver:ISaver
{
    private readonly string _outputDir;

    public VehicleSaver(string outputDir)
    {
        _outputDir = outputDir;

        if (!Directory.Exists(_outputDir))
            Directory.CreateDirectory(_outputDir);
    }

    public void Save(int id, Rect bBox, Mat frame, int frameCount)
    {
        try
        {
            string vehicleDir = Path.Combine(_outputDir, $"vehicle_{id}");
            if (!Directory.Exists(vehicleDir))
                Directory.CreateDirectory(vehicleDir);

            Mat cropped = new Mat(frame, bBox);

            string filename = Path.Combine(vehicleDir, $"frame_{frameCount}.jpg");

            Cv2.ImWrite(filename, cropped);

            Console.WriteLine($"[Saver] Vehicle {id}, frame {frameCount} saved to: {filename}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Saver] Ошибка при сохранении: {ex.Message}");
        }
    }
}