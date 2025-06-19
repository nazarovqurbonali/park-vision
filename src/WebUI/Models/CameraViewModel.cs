using System.ComponentModel.DataAnnotations;

namespace WebUI.Models;

public class CameraViewModel
{
    [Required(ErrorMessage = "Введите RTSP URL")]
    public string RtspUrl { get; set; } = string.Empty;
    public bool IsTimeoutError { get; set; } 

    public bool IsCameraConnected { get; set; }
    public string Message { get; set; } = string.Empty;

    [Range(1, 60, ErrorMessage = "Интервал должен быть от 1 до 60 минут")]
    public int RotationInterval { get; set; } = 5;

    [Range(0, 360, ErrorMessage = "Угол должен быть от 0 до 360 градусов")]
    public int RotationAngle { get; set; } = 10;

    public string RotationDirection { get; set; } = "right";
    public List<RotationLog> RotationLogs { get; set; } = [];
    public List<PtzCommand> PtzCommands { get; set; } = [];
}

public class RotationLog
{
    public DateTime Time { get; set; }
    public int IntervalMinutes { get; set; }
    public int Angle { get; set; }
    public string Direction { get; set; } = string.Empty;
}

public class PtzCommand
{
    public DateTime Time { get; set; }
    public string Command { get; set; } = string.Empty;
}