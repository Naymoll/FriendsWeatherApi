namespace FriendsWeatherApi.Models;

public class Coordinates
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public Coordinates(double latitude, double longitude)
    {
        Longitude = longitude;
        Latitude = latitude;
    }
}