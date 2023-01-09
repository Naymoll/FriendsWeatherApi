using FriendsWeatherApi.Database;
using FriendsWeatherApi.Extensions;
using FriendsWeatherApi.Models;
using FriendsWeatherApi.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FriendsWeatherApi.Controllers;

[ApiController]
[Route("requests/")]
[Authorize(Roles = "Verified", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class RequestsController : ControllerBase
{
    private readonly FriendsWeatherContext _context;
    private readonly ILogger<RequestsController> _logger;

    public RequestsController(FriendsWeatherContext context, ILogger<RequestsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<RequestResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetUserRequests()
    {
        var userId = User.Id();
        var responses = await _context.FriendshipRequests
            .Where(r => r.ReceiverId == userId)
            .Include(r => r.Sender)
            .Select(r => new RequestResponse(r.Id, r.Sender.Login))
            .ToListAsync();

        return Ok(responses);
    }
    
    [HttpGet("{requestId:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetUserRequest(int requestId)
    {
        var userId = User.Id();
        var request = await _context.FriendshipRequests
            .Where(r => r.SenderId == userId && r.Id == requestId)
            .Include(r => r.Sender)
            .FirstOrDefaultAsync();
        if (request is null) return NotFound();

        var response = new RequestResponse(request.Id, request.Sender.Login);
        return Ok(response);
    }
    
    /// <summary>
    /// Отправить запрос дружбы пользователю
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SendRequest(int otherUserId)
    {
        var userId = User.Id();
        if (userId == otherUserId) return BadRequest("You can't send request to yourself");
        
        var request = await _context.FriendshipRequests
            .Where(r => r.SenderId == userId && r.ReceiverId == otherUserId)
            .FirstOrDefaultAsync();
        if (request is not null) return BadRequest("Request already sent");

        var friendshipRequest = new FriendshipRequest
        {
            SenderId = userId,
            ReceiverId = otherUserId,
        };

        await _context.FriendshipRequests.AddAsync(friendshipRequest);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("{requestId:int}/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AcceptRequest(int requestId)
    {
        var userId = User.Id();
        var request = await _context.FriendshipRequests
            .Where(r => r.ReceiverId == userId && r.Id == requestId)
            .FirstOrDefaultAsync();
        if (request is null) return NotFound();
        
        var friendships = new[]
        {
            new Friendship
            {
                UserId = request.SenderId,
                FriendId = request.ReceiverId
            },
            new Friendship
            {
                UserId = request.ReceiverId,
                FriendId = request.SenderId
            },
        };

        _context.FriendshipRequests.Remove(request);
        await _context.Friendships.AddRangeAsync(friendships);
        await _context.SaveChangesAsync();
        return Ok();
    }
    
    [HttpPost("{requestId:int}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RejectRequest(int requestId)
    {
        var userId = User.Id();
        var request = await _context.FriendshipRequests
            .Where(r => r.ReceiverId == userId && r.Id == requestId)
            .FirstOrDefaultAsync();
        if (request is null) return NotFound();
        
        _context.FriendshipRequests.Remove(request);
        await _context.SaveChangesAsync();
        return Ok();
    }
}