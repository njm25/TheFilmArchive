namespace Infrastructure.Email;

// Real SMTP delivery in production (SmtpEmailSender), logged-not-sent in dev
// (ConsoleEmailSender) - which one is registered is decided in Program.cs, so
// nothing above this layer needs to know.
public interface IEmailSender
{
    Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
