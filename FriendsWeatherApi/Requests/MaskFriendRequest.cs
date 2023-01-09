using System.ComponentModel.DataAnnotations;

namespace FriendsWeatherApi.Requests;

public record MaskFriendRequest([Required] string Name, [Required] string Address);