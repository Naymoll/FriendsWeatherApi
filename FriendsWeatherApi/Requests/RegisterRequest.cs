using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FriendsWeatherApi.Requests;

public record RegisterRequest(
    [Required] string Login, 
    [Required] string Email, 
    [Required] string Password, 
    [Required] string Name, 
    [Required] string Address);