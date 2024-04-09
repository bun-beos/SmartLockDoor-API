using Microsoft.AspNetCore.Mvc;

namespace SmartLockDoor.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IFirebaseService _firebaseService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IFirebaseService firebaseService)
        {
            _logger = logger;
            _firebaseService = firebaseService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteImageAsync([FromBody] string imageUrl)
        {
            await _firebaseService.DeleteImageAsync(imageUrl);
            return Ok();
        }
    }
}
