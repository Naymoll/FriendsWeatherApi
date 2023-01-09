using System.ComponentModel.DataAnnotations.Schema;

namespace FriendsWeatherApi.Models;

public class Friendship
{
    public int Id { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [ForeignKey("Friend")]
    public int FriendId { get; set; }
    public User Friend { get; set; } = null!;
    
    [ForeignKey("FriendMask")]
    public int? FriendMaskId { get; set; }
    public UserMask? FriendMask { get; set; }
}
