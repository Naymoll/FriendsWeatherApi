using FriendsWeatherApi.Models;

namespace FriendsWeatherApi.Services;

public interface ITemperatureService
{
    public Task<Weather?> GetTemperatureAsync(Coordinates coordinates);
}