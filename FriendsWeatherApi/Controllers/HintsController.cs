using System.ComponentModel.DataAnnotations;
using FriendsWeatherApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FriendsWeatherApi.Controllers;

[ApiController]
[Route("hints/")]
public class HintsController : ControllerBase
{
    private readonly IGeocodingService _geocodingService;
    private readonly ILogger<HintsController> _logger;

    public HintsController(IGeocodingService service, ILogger<HintsController> logger)
    {
        _geocodingService = service;
        _logger = logger;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetHints([Required] string address)
    {
        var hints = await _geocodingService.GeocodeHintsAsync(address);
        if (hints is null) return StatusCode(StatusCodes.Status500InternalServerError);

        return Ok(hints);
    }
}