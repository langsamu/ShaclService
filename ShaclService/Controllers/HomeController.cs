using Microsoft.AspNetCore.Mvc;

namespace ShaclService.Controllers;

[Route("")]
public class HomeController : Controller
{
    [HttpGet]
    [HttpHead]
    public IActionResult Index() => View();
}