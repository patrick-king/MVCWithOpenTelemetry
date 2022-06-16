using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WeatherSvc;
namespace MVCWithOpenTelemetry.Controllers
{
    public class WeatherController : Controller
    {
        private WeatherSvc.IClient _weatherSvc;
        private ILogger _logger;

        public WeatherController(IClient weatherSvc, 
            ILogger<WeatherController> logger)
        {
            _weatherSvc = weatherSvc;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _weatherSvc.WeatherForecastAsync();
            return View(result);    
        }

        [HttpPost]
        public async Task<IActionResult> Detail(string city)
        {
            IEnumerable<WeatherSvc.WeatherForecast> result;

            if(string.IsNullOrEmpty(city))
            {
                return BadRequest();
            }
            else
            {
                result = await _weatherSvc.WeatherForecast2Async(city);
            }

            return View("Index",result);
        }
    }
}
