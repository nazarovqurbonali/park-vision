namespace Infrastructure.Implementations.VehicleDetection;

public sealed class LiveCameraService(
    ILogger<LiveCameraService> logger) : ILiveCameraService
{
    public event Action<Mat>? FrameReceived;

    private CancellationTokenSource? _cts;
    private VideoCapture? _capture;

    public BaseResult Start(string url)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(Start), date);

        if (_capture != null && _capture.IsOpened())
        {
            logger.OperationCompleted(nameof(Start), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            return BaseResult.Success();
        }

        try
        {
            _capture = new(url);
            if (!_capture.IsOpened())
            {
                logger.OperationCompleted(nameof(Start), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);

                return BaseResult.Failure(ResultPatternError.BadRequest());
            }

            _cts = new CancellationTokenSource();
            Task.Run(() => CaptureLoop(_cts.Token), _cts.Token);

            logger.OperationCompleted(nameof(Start), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            return BaseResult.Success();
        }
        catch (Exception ex)
        {
            logger.OperationException(nameof(Start), ex.Message);
            logger.OperationCompleted(nameof(Start), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            return BaseResult.Failure(ResultPatternError.InternalServerError(ex.Message));
        }
    }

    public BaseResult Stop()
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(Stop), date);

        if (_cts == null)
        {
            logger.OperationCompleted(nameof(Stop), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            return BaseResult.Failure(ResultPatternError.BadRequest());
        }

        _cts.Cancel();
        _capture?.Dispose();
        _cts.Dispose();
        _capture = null;
        _cts = null;

        logger.OperationCompleted(nameof(Stop), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
        return BaseResult.Success();
    }

    private void CaptureLoop(CancellationToken token)
    {
        Mat frame = new();
        while (!token.IsCancellationRequested)
        {
            if (_capture is null) continue;
            if (_capture.Read(frame) && !frame.Empty())
                FrameReceived?.Invoke(frame);
        }
    }
}