namespace FriendsWeatherApi.Models;

public record Weather
{
    public double TemperatureC { get; set; }

    public double TemperatureF { get; set; }
    
    public Weather(double temperatureC)
    {
        TemperatureC = temperatureC;
        TemperatureF = 32.0 + (TemperatureC / 0.5556);
    }
}