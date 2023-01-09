using MailKit.Net.Smtp;
using MimeKit;

namespace FriendsWeatherApi.Services;

public class EmailSender : IEmailSender
{
    private readonly MailboxAddress _from;
    private readonly ILogger<EmailSender> _logger;
    private readonly string _password;
    
    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
    {
        _password = configuration["Mail:Password"] ?? throw new InvalidOperationException("Mail:Password isn't presented");
        var email = configuration["Mail:From"] ?? throw new InvalidOperationException("Mail:From isn't presented");
        
        _from = new MailboxAddress("FWSupport", email);
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string subject, string address, string name, string content)
    {
        try
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(_from);
        
            var to = new MailboxAddress(name, address);
            emailMessage.To.Add(to);
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text)
            {
                Text = content
            };

            using var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            client.Timeout = 5 * 1000;

            await client.ConnectAsync("smtp.yandex.ru", 465, true);
            await client.AuthenticateAsync(_from.Address, _password);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return false;
        }

        return true;
    }
}