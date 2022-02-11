using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace SAIOT_web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<WeatherForecast> Get()
        {
            string fileContent = "";
            if (System.IO.File.Exists("wheather"))
            {
                fileContent = System.IO.File.ReadAllText("wheather");
            }
            if (fileContent == "")
                fileContent = "24 36";

            string[] latlong = fileContent.Split(" ");

            WheatherAPI wheather = new WheatherAPI();
            using (var httpClient = new HttpClient())
            {
                string url = "http://fcc-weather-api.glitch.me/api/current?lat=" + latlong[0] + "&lon=" + latlong[1];
                string apiResponse = await GetExternalResponse(url);
                wheather = JsonConvert.DeserializeObject<WheatherAPI>(apiResponse);
            }

            return new WeatherForecast
            {
                Date = DateTime.Now.ToString("dd-MM-yyyy"),
                TemperatureC = wheather.main.temp,
                Summary = fileContent
            };
        }

        [HttpGet("{id}")]
        public async Task<WeatherForecast> GetTodoItem(int id)
        {
            string fileContent = "";
            if (id == 1)
            {
                if (System.IO.File.Exists("wheather"))
                {
                    fileContent = System.IO.File.ReadAllText("wheather");
                }
                if (fileContent == "")
                    fileContent = "24 36";

                string[] latlong = fileContent.Split(" ");

                WheatherAPI wheather = new WheatherAPI();
                using (var httpClient = new HttpClient())
                {
                    string url = "http://fcc-weather-api.glitch.me/api/current?lat=" + latlong[0] + "&lon=" + latlong[1];
                    string apiResponse = await GetExternalResponse(url);
                    wheather = JsonConvert.DeserializeObject<WheatherAPI>(apiResponse);
                }

                return new WeatherForecast{
                    Date = DateTime.Now.ToString("dd-MM-yyyy"),
                    TemperatureC = wheather.main.temp,
                    Summary = fileContent
                };
                
            }
            if (System.IO.File.Exists("account"))
            {
                fileContent = System.IO.File.ReadAllText("account");
            }
            return new WeatherForecast
                {
                    Date = DateTime.Now.ToString("dd-MM-yyyy"),
                    Summary = "11"
            };

        }

        [HttpPost]
        public async Task<ActionResult<string>> Followers(AddInfo addItem)
        {
            try
            {
                string type = addItem.isWeather ? "wheather" : "account";

                if (System.IO.File.Exists(type))
                {
                    System.IO.File.Delete(type);
                }
                using (FileStream fs = System.IO.File.Create(type))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(addItem.info);
                    fs.Write(info, 0, info.Length);
                }
            }
            catch (Exception ex)
            {

            }
            return "test";
        
        }

        private async Task<string> GetExternalResponse(string _address)
        {

            HttpClient httpClient = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, _address);
            requestMessage.Headers.Add("User-Agent", "User-Agent-Here");
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
            var result = response.Content.ReadAsStringAsync().Result;
            return result;
        }
    }
}