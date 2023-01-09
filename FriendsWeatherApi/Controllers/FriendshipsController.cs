using FriendsWeatherApi.Database;
using FriendsWeatherApi.Extensions;
using FriendsWeatherApi.Models;
using FriendsWeatherApi.Requests;
using FriendsWeatherApi.Responses;
using FriendsWeatherApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FriendsWeatherApi.Controllers;

[ApiController]
[Route("friends/")]
[Authorize(Roles = "Verified", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class FriendshipsController : ControllerBase
{
    private readonly FriendsWeatherContext _context;
    private readonly IGeocodingService _service;
    private readonly ILogger<FriendshipsController> _logger;

    public FriendshipsController(FriendsWeatherContext context, IGeocodingService service, ILogger<FriendshipsController> logger)
    {
        _context = context;
        _service = service;
        _logger = logger;
    }

    private FriendResponse ToResponse(Friendship friendship)
    {
        var friend = friendship.Friend;
        var (name, address) = friendship.FriendMask switch
        {
            null => (friend.Name, friend.Address),
            { Name: var maskName, Address: var maskAddress } => (maskName, maskAddress),
        };

        return new FriendResponse(friend.Id, friend.Login, name, address);
    }
    
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetUserFriends()
    {
        var userId = User.Identity!.Name!.Id();
        var friendships = await _context.Friendships
            .Where(f => f.UserId == userId)
            .Include(f => f.Friend)
            .Include(f => f.FriendMask)
            .ToListAsync();

        var friends = friendships.Select(ToResponse);
        return Ok(friends);
    }
    
    [HttpGet("{friendId:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FriendResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetUserFriend(int friendId)
    {
        var userId = User.Identity!.Name!.Id();
        var friendship = await _context.Friendships
            .Where(f => f.UserId == userId && f.FriendId == friendId)
            .Include(f => f.Friend)
            .Include(f => f.FriendMask)
            .FirstOrDefaultAsync();
        if (friendship is null) return NotFound();

        var response = ToResponse(friendship);
        return Ok(response);
    }
    
    /// <summary>
    /// Изменить имя и адрес друга
    /// </summary>
    /// <returns></returns>
    [HttpPost("{friendId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> MaskUserFriend(int friendId, MaskFriendRequest request)
    {
        var userId = User.Identity!.Name!.Id();
        var friendship = await _context.Friendships
            .Where(f => f.UserId == userId && f.FriendId == friendId)
            .Include(f => f.FriendMask)
            .FirstOrDefaultAsync();
        if (friendship is null) return NotFound();
        
        var coordinates = await _service.GeocodeAsync(request.Address);
        if (coordinates is null) return StatusCode(StatusCodes.Status500InternalServerError);

        var mask = friendship.FriendMask ?? new UserMask();
        mask.Name = request.Name;
        mask.Address = request.Address;
        mask.Latitude = coordinates.Latitude;
        mask.Longitude = coordinates.Longitude;

        if (friendship.FriendMask is null)
        {
            await _context.Masks.AddAsync(mask);
            friendship.FriendMask = mask;
            _context.Friendships.Update(friendship);
        }
        else
        {
            _context.Masks.Update(mask);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }
    
    [HttpDelete("{friendId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveFriend(int friendId)
    {
        var userId = User.Identity!.Name!.Id();
        var friendship = await _context.Friendships
            .Where(f => f.UserId == userId && f.FriendId == friendId)
            .FirstOrDefaultAsync();
        if (friendship is null) return NotFound();

        _context.Friendships.Remove(friendship);
        await _context.SaveChangesAsync();
        return Ok();
    }
}