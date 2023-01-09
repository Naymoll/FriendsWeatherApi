using FriendsWeatherApi.Models;

namespace FriendsWeatherApi.Services;

public interface IGeocodingService
{
    public Task<Coordinates?> GeocodeAsync(string address);
    public Task<IEnumerable<string>?> GeocodeHintsAsync(string address);
}