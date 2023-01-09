using FriendsWeatherApi.Database;
using FriendsWeatherApi.Extensions;
using FriendsWeatherApi.Models;
using FriendsWeatherApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FriendsWeatherApi.Controllers;

[ApiController]
[Route("weather/")]
[Authorize(Roles = "Verified", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class WeatherController : ControllerBase
{
    private readonly FriendsWeatherContext _context;
    private readonly ITemperatureService _temperatureService;
    private readonly ILogger<WeatherController> _logger;
    
    public WeatherController(FriendsWeatherContext context, ITemperatureService temperatureService, ILogger<WeatherController> logger)
    {
        _context = context;
        _temperatureService = temperatureService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Weather), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Weather>> GetFriendsTemperature()
    {
        var userId = User.Id();
        var friendships = await _context.Friendships.Where(f => f.UserId == userId)
            .Include(f => f.Friend)
            .Include(f => f.FriendMask)
            .ToListAsync();
        
        if (friendships.Count == 0) return Ok("You have no friends (ㅠ﹏ㅠ)");

        var coordinates = friendships.Select(f =>
        {
            var friend = f.Friend;
            var coordinates = f.FriendMask switch
            {
                null => new Coordinates(friend.Latitude, friend.Longitude),
                { Latitude: var latitude, Longitude: var longitude } =>
                    new Coordinates(latitude, longitude),
            };

            return coordinates;
        }).ToList();


        var weathers = new List<Weather>();
        foreach (var coordinate in coordinates)
        {
            var weather = await _temperatureService.GetTemperatureAsync(coordinate);
            if (weather is null) return StatusCode(StatusCodes.Status500InternalServerError);
            
            weathers.Add(weather);
        }
        var avg = weathers.Average(w => w.TemperatureC);

        return Ok(new Weather(avg));
    }
}