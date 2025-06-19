namespace WebUI.Controllers;

public class SystemController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Система";
        return View();
    }

}