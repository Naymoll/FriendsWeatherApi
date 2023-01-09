using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using FriendsWeatherApi.Database;
using FriendsWeatherApi.Extensions;
using FriendsWeatherApi.Models;
using FriendsWeatherApi.Requests;
using FriendsWeatherApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FriendsWeatherApi.Controllers;

[ApiController]
[Route("/")]
public class AccountController : ControllerBase
{
    private readonly FriendsWeatherContext _context;
    private readonly Hasher _hasher;
    private readonly ILogger<AccountController> _logger;
    private readonly AuthConfiguration _config;
    private readonly IGeocodingService _geocodingService;
    private readonly IEmailSender _sender;

    public AccountController(
        FriendsWeatherContext context, 
        IEmailSender sender, 
        IGeocodingService service, 
        AuthConfiguration config, 
        ILogger<AccountController> logger)
    {
        _context = context;
        _geocodingService = service;
        _hasher = new Hasher(512, 100_000, HashAlgorithmName.SHA512);
        _config = config;
        _logger = logger;
        _sender = sender;
    }

    private string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimsIdentity.DefaultNameClaimType, Convert.ToString(user.Id)),
            new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Status.ToString())
        };
        var securityKey = _config.SecurityKey();
        var credential = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _config.Issuer,
            _config.Audience,
            claims,
            DateTime.Now,
            DateTime.Now.AddDays(30),
            credential
        );

        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenStr;
    }
    
    /// <summary>
    /// Отправить письмо верификации на почту
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = "Unverified", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SendEmail()
    {
        var userId = User.Id();
        var user = await _context.Users
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync();
        if (user is null) return NotFound();

        var verifytoken = await _context.VerifyTokens
            .Where(t => t.UserId == user.Id)
            .FirstOrDefaultAsync();
        
        // Удаляем пред. токен
        if (verifytoken is not null) _context.VerifyTokens.Remove(verifytoken);

        var token = new VerifyToken
        {
            Token = Guid.NewGuid().ToString(),
            Until = DateTime.Now.AddDays(2),
            User = user
        };

        var req = HttpContext.Request;
        var url = new UriBuilder(req.Scheme, req.Host.Host, req.Host.Port ?? -1);
        var content = $"Follow the link to confirm the email: {url}verify?token={token.Token}";
        var okay = await _sender.SendEmailAsync("Email verification", user.Email, user.Name, content);

        if (okay)
        {
            await _context.VerifyTokens.AddAsync(token);
            await _context.SaveChangesAsync();
            return Ok();
        }
        else
        {
            _logger.LogWarning($"Failed to send email to {user.Email}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
    /// <summary>
    /// Верифицировать почту
    /// </summary>
    /// <returns></returns>
    [HttpPost("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Verify(string token)
    {
        var verifyToken = await _context.VerifyTokens
            .Where(t => t.Token == token)
            .Include(t => t.User)
            .FirstOrDefaultAsync();
        
        if (verifyToken is null) return NotFound();
        if (verifyToken.Until < DateTime.Now) return BadRequest("Token expired");

        var user = verifyToken.User;
        user.Status = VerifyStatus.Verified;
        _context.Users.Update(user);

        _context.VerifyTokens.Remove(verifyToken);
        await _context.SaveChangesAsync();
        return Ok();
    }
    
    
    [AllowAnonymous]
    [HttpPost("login")]
    [Produces("text/plain")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users.Where(u => u.Login == request.Login).FirstOrDefaultAsync();
        if (user is null) return Unauthorized("User not found");

        var salt = user.Salt;
        var hash = user.PasswordHash;
        var equal = _hasher.VerifyPassword(request.Password, Convert.FromHexString(hash), Convert.FromHexString(salt));

        if (!equal) return Unauthorized("Invalid password");
        
        _logger.LogInformation($"User {user.Login} logged in");
        var token = GenerateToken(user);
        return Ok(token);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [Produces("text/plain")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<ActionResult> Register(RegisterRequest request)
    {
        var user = await _context.Users.Where(u => u.Login == request.Login).FirstOrDefaultAsync();
        if (user is not null) return Unauthorized("User already exists");

        var coordinates = await _geocodingService.GeocodeAsync(request.Address);
        if (coordinates is null) return StatusCode(StatusCodes.Status500InternalServerError);
        
        var hash = _hasher.HashPassword(request.Password, out var salt);
        user = new User
        {
            Login = request.Login,
            Email = request.Email,
            PasswordHash = Convert.ToHexString(hash),
            Salt = Convert.ToHexString(salt),
            Name = request.Name,
            Address = request.Address,
            Latitude = coordinates.Latitude,
            Longitude = coordinates.Longitude,
            Status = VerifyStatus.Unverified,
        };
        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"New user {user.Login} registered");
        return Ok();
    }
}