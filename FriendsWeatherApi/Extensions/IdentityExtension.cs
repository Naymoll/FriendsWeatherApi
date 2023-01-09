using System.Security.Claims;

namespace FriendsWeatherApi.Extensions;

public static class IdentityExtension
{
    public static int Id(this ClaimsPrincipal user)
    {
        return Convert.ToInt32(user.Identity!.Name);
    }
}