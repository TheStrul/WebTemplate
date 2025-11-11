namespace WebTemplate.Core.Services
{
    using System.Threading.Tasks;
    using WebTemplate.Core.Interfaces;

    public class NoOpEmailSender : IEmailSender
    {
        public Task SendAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null, CancellationToken cancellationToken = default)
        {
            // Intentionally do nothing in NoOp mode
            return Task.CompletedTask;
        }
    }
}
