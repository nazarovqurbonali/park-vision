using OpenCvSharp;

namespace WebUI.Controllers;

public class CameraController : Controller
{
    private readonly ILiveCameraService _liveCameraService;
    private readonly IVideoProcessor _videoProcessor;
    private readonly IVehicleDetectionService _vehicleDetectionService;
    private readonly ILogger<CameraController> _logger;
    private byte[] _latestFrameBytes = [];

    public CameraController(
        ILiveCameraService liveCameraService,
        IVideoProcessor videoProcessor,
        IVehicleDetectionService vehicleDetectionService,
        ILogger<CameraController> logger)
    {
        _liveCameraService = liveCameraService;
        _videoProcessor = videoProcessor;
        _vehicleDetectionService = vehicleDetectionService;
        _logger = logger;

        // Подписка на кадры
        _liveCameraService.FrameReceived += OnFrameReceived;
    }

    private void OnFrameReceived(Mat frame)
    {
        try
        {
            if (!frame.Empty())
            {
                _latestFrameBytes = frame.ToBytes(".jpg");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка обработки кадра: {ex.Message}");
        }
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Камера";
        return View(new CameraViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> CheckCamera(CameraViewModel model)
    {
        ViewData["Title"] = "Камера";
        if (!ModelState.IsValid)
        {
            model.Message = "Пожалуйста, введите корректный RTSP URL.";
            return View("Index", model);
        }

        try
        {
            _logger.LogInformation($"Проверка подключения камеры: {model.RtspUrl}");
            var checkResult = _videoProcessor.CheckCameraAvailability(model.RtspUrl);
            if (checkResult.IsSuccess)
            {
                var startResult = _liveCameraService.Start(model.RtspUrl);
                if (startResult.IsSuccess)
                {
                    model.IsCameraConnected = true;
                    model.Message = "Камера успешно подключена!";
                    _logger.LogInformation("Камера успешно подключена.");
                }
                else
                {
                    model.IsCameraConnected = false;
                    model.Message = "Камера не найдена: ошибка запуска потока.";
                    model.IsTimeoutError =
                        startResult.Error.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase);
                    _logger.LogError($"Ошибка запуска потока: {startResult.Error.Message}");
                }
            }
            else
            {
                model.IsCameraConnected = false;
                model.Message = "Камера не найдена: устройство недоступно.";
                model.IsTimeoutError =
                    checkResult.Error.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase);
                _logger.LogError($"Ошибка подключения: {checkResult.Error.Message}");
            }
        }
        catch (Exception ex)
        {
            model.IsCameraConnected = false;
            model.Message = "Камера не найдена: внутренняя ошибка.";
            model.IsTimeoutError = ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase);
            _logger.LogError($"Внутренняя ошибка при подключении камеры: {ex.Message}");
        }

        return View("Index", model);
    }

    [HttpPost]
    public IActionResult AddRotation(CameraViewModel model)
    {
        ViewData["Title"] = "Камера";
        if (string.IsNullOrEmpty(model.RtspUrl))
        {
            model.Message = "Сначала подключите камеру.";
            return View("Index", model);
        }

        model.RotationLogs.Add(new RotationLog
        {
            Time = DateTime.Now,
            IntervalMinutes = model.RotationInterval,
            Angle = model.RotationAngle,
            Direction = model.RotationDirection
        });
        model.Message =
            $"Поворот добавлен: каждые {model.RotationInterval} мин на {model.RotationAngle}° ({model.RotationDirection})";
        _logger.LogInformation(
            $"Добавлен поворот: {model.RotationInterval} мин, {model.RotationAngle}°, {model.RotationDirection}");

        return View("Index", model);
    }

    [HttpPost]
    public IActionResult PtzControl([FromBody] PtzRequest request)
    {
        _logger.LogInformation($"PTZ команда: {request.Command}");
        var model = new CameraViewModel();
        model.PtzCommands.Add(new PtzCommand
        {
            Time = DateTime.Now,
            Command = request.Command
        });
        return Json(new { success = true, message = $"Команда PTZ: {request.Command}" });
    }

    [HttpGet]
    public IActionResult GetFrame()
    {
        if (_latestFrameBytes.Length > 0)
        {
            return File(_latestFrameBytes, "image/jpeg");
        }

        _logger.LogWarning("Кадр недоступен.");
        return File(new byte[0], "image/jpeg");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _liveCameraService.FrameReceived -= OnFrameReceived;
            _liveCameraService.Stop();
        }

        base.Dispose(disposing);
    }
}

public class PtzRequest
{
    public string Command { get; set; }
}