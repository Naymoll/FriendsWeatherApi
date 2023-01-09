using System.Text.Json;
using FriendsWeatherApi.Models;

namespace FriendsWeatherApi.Services;

public class OpenWeatherService : ITemperatureService
{
    private readonly string _token;
    private readonly HttpClient _client;
    private readonly ILogger<OpenWeatherService> _logger;
    
    public OpenWeatherService(HttpClient client, IConfiguration configuration, ILogger<OpenWeatherService> logger)
    {
        _client = client;
        _token = configuration["OpenWeatherToken"] ?? throw new InvalidOperationException("OpenWeatherToken isn't presented");
        _logger = logger;
    }

    public async Task<Weather?> GetTemperatureAsync(Coordinates coordinates)
    {
        try
        {
            var relative = $"?lat={coordinates.Latitude}&lon={coordinates.Longitude}&units=metric&appid={_token}";
            var fullUrl = new Uri(_client.BaseAddress!, relative);
            var response = await _client.GetAsync(fullUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Weather service response not Ok. Returned {response}");
                return null;
            }
            
            var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
            var root = json!.RootElement;
            var main = root.GetProperty("main");
            var temperature = main.GetProperty("temp").GetDouble();
            return new Weather(temperature);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
            return null;
        }
    }
}