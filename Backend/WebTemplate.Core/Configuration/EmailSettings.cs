namespace WebTemplate.Core.Configuration
{
    using System.ComponentModel.DataAnnotations;

    public class EmailSettings
    {
        public const string SectionName = "Email";

        [Required]
        public string Provider { get; set; } = "Smtp"; // Smtp | SendGrid | Custom

        // Common
        public string From { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;

        // SMTP
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public bool SmtpEnableSsl { get; set; } = true;
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;

        // SendGrid
        public string SendGridApiKey { get; set; } = string.Empty;
    }
}
