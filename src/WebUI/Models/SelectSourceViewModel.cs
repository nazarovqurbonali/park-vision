using System.ComponentModel.DataAnnotations;

namespace WebUI.Models;

public class SelectSourceViewModel
{
    [Display(Name = "Путь к локальному видео")]
    [Required(ErrorMessage = "Укажите путь к видео или выберите камеру")]
    public string VideoPath { get; set; } = string.Empty;

    [Display(Name = "IP-адрес камеры (Ethernet)")]
    [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "Неверный формат IP-адреса")]
    public string CameraIp { get; set; } = string.Empty;

    [Display(Name = "Использовать камеру вместо видео")]
    public bool UseCamera { get; set; }
}