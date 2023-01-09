namespace FriendsWeatherApi.Models;

public enum VerifyStatus 
{
    Unverified = 0,
    Verified,
}

public class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;
    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;
    public string Salt { get; set; } = null!;
    
    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    public VerifyStatus Status { get; set; }
}

