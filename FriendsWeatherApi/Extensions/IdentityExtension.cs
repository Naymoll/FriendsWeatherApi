namespace FriendsWeatherApi.Extensions;

public static class IdentityExtension
{
    public static int Id(this string str)
    {
        return Convert.ToInt32(str);
    }
}