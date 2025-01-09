using Microsoft.AspNetCore.Mvc;

namespace ShaclService;

[Route("")]
public class HomeController : Controller
{
    [HttpGet]
    [HttpHead]
    public IActionResult Index() => this.View();
}