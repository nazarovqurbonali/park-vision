namespace Infrastructure.Implementation.VehicleDetection;

public sealed class VideoSource(string uri) : IVideoSource
{
    private VideoCapture _capture = null!;

    public bool Open()
    {
        _capture = new VideoCapture(uri);
        return _capture.IsOpened();
    }

    public bool ReadFrame(out Mat frame)
    {
        frame = new Mat();
        return _capture.Read(frame) && !frame.Empty();
    }

    public void Release() => _capture.Release();

    public void Dispose() => _capture.Dispose();
}