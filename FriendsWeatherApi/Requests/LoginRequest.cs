using System.ComponentModel.DataAnnotations;

namespace FriendsWeatherApi.Requests;

public record LoginRequest([Required] string Login, [Required] string Password);