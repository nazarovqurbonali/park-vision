namespace Infrastructure.Implementations.VehicleDetection;

public sealed class VehicleSaver : ISaver
{
    private readonly string _outputDir;

    public VehicleSaver(string outputDir)
    {
        _outputDir = outputDir ?? throw new ArgumentNullException(nameof(outputDir));
        if (!Directory.Exists(_outputDir))
            Directory.CreateDirectory(_outputDir);
    }

    public BaseResult Save(int id, Rect bBox, Mat frame, int frameCount)
    {
        try
        {
            string vehicleDir = Path.Combine(_outputDir, $"vehicle_{id}");
            if (!Directory.Exists(vehicleDir))
                Directory.CreateDirectory(vehicleDir);

            using var cropped = new Mat(frame, bBox);
            string filename = Path.Combine(vehicleDir, $"frame_{frameCount}.jpg");
            Cv2.ImWrite(filename, cropped);
            return BaseResult.Success($"Saved: {filename}");
        }
        catch (Exception ex)
        {
            return BaseResult.Failure(ResultPatternError.InternalServerError($"Save error: {ex.Message}"));
        }
    }
}