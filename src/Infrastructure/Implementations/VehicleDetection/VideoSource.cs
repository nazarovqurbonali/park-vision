namespace Infrastructure.Implementations.VehicleDetection;

public sealed class VideoSource(string uri) : IVideoSource
{
    private VideoCapture _capture = null!;

    public Result<bool> Open()
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(uri);
            _capture = new VideoCapture(uri);
            if (!_capture.IsOpened())
                return Result<bool>.Failure(ResultPatternError.BadRequest("Failed to open video source."));
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                ResultPatternError.InternalServerError($"Error opening video: {ex.Message}"));
        }
    }

    public Result<bool> ReadFrame(out Mat frame)
    {
        frame = new Mat();
        try
        {
            if (!_capture.IsOpened())
                return Result<bool>.Failure(ResultPatternError.BadRequest("Video source not opened."));
            bool success = _capture.Read(frame) && !frame.Empty();
            if (!success)
                return Result<bool>.Failure(ResultPatternError.BadRequest("Failed to read frame."));
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            frame.Dispose();
            return Result<bool>.Failure(
                ResultPatternError.InternalServerError($"Error reading frame: {ex.Message}"));
        }
    }

    public BaseResult Release()
    {
        try
        {
            _capture.Release();
            return BaseResult.Success("Video source released.");
        }
        catch (Exception ex)
        {
            return BaseResult.Failure(
                ResultPatternError.InternalServerError($"Error releasing video: {ex.Message}"));
        }
    }
}