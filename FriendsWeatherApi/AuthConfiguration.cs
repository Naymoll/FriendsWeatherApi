using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FriendsWeatherApi;

public class AuthConfiguration
{
    public string Key { get; }
    public string Issuer { get; } 
    public string Audience { get; }

    public AuthConfiguration(IConfiguration configuration)
    {
        Key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key isn't presented");
        Issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer isn't presented");
        Audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience isn't presented");
    }

    public SymmetricSecurityKey SecurityKey()
    {
        var bytes = Encoding.UTF8.GetBytes(Key);
        return new SymmetricSecurityKey(bytes);
    }
}