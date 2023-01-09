using System.Text.Json;
using System.Text.Json.Serialization;
using FriendsWeatherApi.Models;

namespace FriendsWeatherApi.Services;

record Features
{
    [JsonPropertyName("relevance")] 
    public double Relevance { get; set; }

    [JsonPropertyName("center")] 
    public double[] Center { get; set; } = null!;

    [JsonPropertyName("place_name")] 
    public string Address { get; set; } = null!;
}

public class MapboxGeocodingService : IGeocodingService
{
    private readonly string _token;
    private readonly HttpClient _client;
    private readonly ILogger<MapboxGeocodingService> _logger;
    
    
    public MapboxGeocodingService(HttpClient client, IConfiguration configuration, ILogger<MapboxGeocodingService> logger)
    {
        _client = client;
        _token = configuration["MapboxToken"] ?? throw new InvalidOperationException("MapboxToken isn't presented");
        _logger = logger;
    }

    private static List<Features>? Deserialize(JsonDocument json)
    {
        var root = json.RootElement;
        var property = root.GetProperty("features");
        return property.Deserialize<List<Features>>();
    }

    public async Task<Coordinates?> GeocodeAsync(string address)
    {
        try
        {
            var fullUrl = new Uri(_client.BaseAddress!, $"{address}.json?access_token={_token}");
            var response = await _client.GetAsync(fullUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Geocode response not Ok. Returned {response}");
                return null;
            }
            
            var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
            var features = Deserialize(json!);
            var relevant = features?.First(); // На первом месте наиболее релевантный 

            return relevant == null ? null : new Coordinates(relevant.Center[1], relevant.Center[0]);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
            return null;
        }
    }

    public async Task<IEnumerable<string>?> GeocodeHintsAsync(string address)
    {
        try
        {
            var fullUrl = new Uri(_client.BaseAddress!, $"{address}.json?access_token={_token}");
            var response = await _client.GetAsync(fullUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Geocode response not Ok. Returned {response}");
                return null;
            }
            
            var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
            var features = Deserialize(json!);
            var hints = features?.Select(f => f.Address);
            return hints;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
            return null;
        }
    }
}