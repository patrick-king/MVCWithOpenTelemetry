using Microsoft.AspNetCore.Mvc;
using MVCWithOpenTelemetry.Models;
using System.Diagnostics;

namespace MVCWithOpenTelemetry.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            _logger.Log(LogLevel.Information, "Viewing Privacy Policy");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult GenerateError()
        {
            throw new ApplicationException("Unable to understand this request. An Exception is being thrown");
        }
    }
}