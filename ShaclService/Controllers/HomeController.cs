﻿namespace ShaclService
{
    using Microsoft.AspNetCore.Mvc;

    public class HomeController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return this.View();
        }
    }
}