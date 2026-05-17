using Clothy.NotificationService.BLL.Configuration;
using Clothy.NotificationService.BLL.DTOs;
using Clothy.NotificationService.BLL.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Clothy.NotificationService.BLL.Services;

public class GmailProvider : IEmailProvider
{
    private readonly string fromEmail;
    private readonly string fromName;
    private readonly string appPassword;

    public GmailProvider(IOptions<EmailProviderOptions> options)
    {
        fromEmail = options.Value.FromEmail!;
        fromName = options.Value.FromName!;
        appPassword = options.Value.ApiKey!;
    }

    public async Task SendEmailAsync(EmailMessageDTO emailMessageDTO, CancellationToken cancellationToken = default)
    {
        MimeMessage message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(emailMessageDTO.To));
        message.Subject = emailMessageDTO.Subject;
        message.Body = new TextPart("html") { Text = emailMessageDTO.HtmlBody };

        using SmtpClient smtp = new SmtpClient();
        await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls, cancellationToken);
        await smtp.AuthenticateAsync(fromEmail, appPassword, cancellationToken);
        await smtp.SendAsync(message, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }
}