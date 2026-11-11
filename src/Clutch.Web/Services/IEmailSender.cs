using System.Net;
using System.Net.Mail;

namespace Clutch.Web.Services;

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
}

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        var host = _config["Smtp:Host"];
        var portStr = _config["Smtp:Port"];
        var from = _config["Smtp:From"] ?? "no-reply@clutch.local";
        var user = _config["Smtp:User"];
        var password = _config["Smtp:Password"];

        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogInformation("SMTP not configured. Email to {ToEmail} skipped: {Subject}", toEmail, subject);
            return;
        }

        int port = int.TryParse(portStr, out var p) ? p : 587;

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = port == 587 || port == 465,
            Credentials = !string.IsNullOrWhiteSpace(user) ? new NetworkCredential(user, password) : null
        };

        var mail = new MailMessage
        {
            From = new MailAddress(from, "Clutch Hackathons"),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        mail.To.Add(toEmail);

        try
        {
            await client.SendMailAsync(mail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
        }
    }
}
