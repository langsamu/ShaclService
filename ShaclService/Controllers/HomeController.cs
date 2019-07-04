namespace ShaclService
{
    using Microsoft.AspNetCore.Mvc;

    [Route("")]
    public class HomeController : Controller
    {
        [HttpGet]
        [HttpHead]
        public IActionResult Index()
        {
            return this.View();
        }
    }
}