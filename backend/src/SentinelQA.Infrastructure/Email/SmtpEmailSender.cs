using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SentinelQA.Application.Abstractions.Infrastructure;

namespace SentinelQA.Infrastructure.Email;

public sealed class SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var host = configuration["Email:Host"] ?? "localhost";
        var port = int.Parse(configuration["Email:Port"] ?? "1025");
        var from = configuration["Email:From"] ?? "noreply@sentinelqa.local";

        using var message = new MailMessage(from, to, subject, htmlBody) { IsBodyHtml = true };
        using var client = new SmtpClient(host, port);

        var username = configuration["Email:Username"];
        if (!string.IsNullOrWhiteSpace(username))
            client.Credentials = new NetworkCredential(username, configuration["Email:Password"]);

        await client.SendMailAsync(message, cancellationToken);
        logger.LogInformation("Email sent to {Recipient}: {Subject}", to, subject);
    }
}