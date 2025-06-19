namespace WebUI.Controllers;

public class VideoController(IConfiguration config) : Controller
{
    [HttpGet]
    public IActionResult SelectSource()
    {
        var model = new SelectSourceViewModel
        {
            VideoPath = config["Paths:VideoPath"],
            CameraIp = config["Paths:CameraIp"],
            UseCamera = false
        };
        return View(model);
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SelectSource(SelectSourceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string source = model.UseCamera ? model.CameraIp : model.VideoPath;
        return RedirectToAction(nameof(Stream), new { source });
    }

  
    [HttpGet]
    public IActionResult Stream(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return RedirectToAction(nameof(SelectSource));
        }
        var model = new StreamViewModel { Source = source };
        return View(model);
    }
}