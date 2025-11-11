using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using WebTemplate.Core.Configuration;
using WebTemplate.Core.Interfaces;

namespace WebTemplate.Core.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        public SmtpEmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null, CancellationToken cancellationToken = default)
        {
            using var message = new MailMessage();
            message.From = new MailAddress(string.IsNullOrWhiteSpace(_settings.From) ? _settings.SmtpUser : _settings.From, _settings.FromName);
            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = htmlBody;
            message.IsBodyHtml = true;

            if (!string.IsNullOrEmpty(plainTextBody))
            {
                var altView = AlternateView.CreateAlternateViewFromString(plainTextBody, null, "text/plain");
                message.AlternateViews.Add(altView);
            }

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.SmtpEnableSsl,
                Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPassword)
            };

            await client.SendMailAsync(message, cancellationToken);
        }
    }
}
