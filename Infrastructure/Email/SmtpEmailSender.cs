using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _fromAddress;
    private readonly string _fromName;

    public SmtpEmailSender(IConfiguration configuration)
    {
        _host = configuration["Email:Smtp:Host"]
            ?? throw new InvalidOperationException("Email:Smtp:Host is not configured.");
        _port = configuration.GetValue<int?>("Email:Smtp:Port")
            ?? throw new InvalidOperationException("Email:Smtp:Port is not configured.");
        _username = configuration["Email:Smtp:Username"]
            ?? throw new InvalidOperationException("Email:Smtp:Username is not configured.");
        _password = configuration["Email:Smtp:Password"]
            ?? throw new InvalidOperationException("Email:Smtp:Password is not configured.");
        _fromAddress = configuration["Email:Smtp:FromAddress"]
            ?? throw new InvalidOperationException("Email:Smtp:FromAddress is not configured.");
        _fromName = configuration["Email:Smtp:FromName"] ?? _fromAddress;
    }

    public async Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromAddress));
        message.To.Add(MailboxAddress.Parse(toAddress));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(_username, _password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
