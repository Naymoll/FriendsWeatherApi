namespace FriendsWeatherApi.Services;

public interface IEmailSender
{
    public Task<bool> SendEmailAsync(string subject, string address, string name, string content);
}