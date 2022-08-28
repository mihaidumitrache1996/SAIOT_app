namespace SAIOT_web
{
    public class WeatherForecast
    {
        public string Date { get; set; }

        public double? TemperatureC { get; set; }

        public int? TemperatureF => TemperatureC != null ? 32 + (int)(TemperatureC / 0.5556) : null;

        public string? Summary { get; set; }
    }

    public class AddInfo
    {
        public bool isWeather { get; set; }
        public string info { get; set; }
    }

    public class WheatherAPI
    {
        public sysClass sys { get; set; }
        public string name { get; set; }

        public mainClass main { get; set; }
    }
    public class sysClass
    {
        public string country { get; set; }
    }

    public class mainClass
    {
        public double temp { get; set; }
    }
}