using System.ComponentModel.DataAnnotations.Schema;

namespace FriendsWeatherApi.Models;

public class FriendshipRequest
{
    public int Id { get; set; }
    
    [ForeignKey("Sender")]
    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;

    [ForeignKey("Receiver")]
    public int ReceiverId { get; set; }
    public User Receiver { get; set; } = null!;
}