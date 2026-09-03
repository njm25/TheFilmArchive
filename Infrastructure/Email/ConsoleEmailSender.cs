using Microsoft.Extensions.Logging;

namespace Infrastructure.Email;

// Dev implementation - logs instead of sending, so local development never
// dispatches real mail. The registration link is in the logged body, which is
// how you complete a signup locally.
public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Email to {ToAddress}: {Subject}\n{Body}", toAddress, subject, htmlBody);
        return Task.CompletedTask;
    }
}
