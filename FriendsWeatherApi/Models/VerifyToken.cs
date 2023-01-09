using System.ComponentModel.DataAnnotations.Schema;

namespace FriendsWeatherApi.Models;

public class VerifyToken
{
    public int Id { get; set; }
    
    [ForeignKey("User")]
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public DateTime Until { get; set; }
    
    public string Token { get; set; } = null!;
}