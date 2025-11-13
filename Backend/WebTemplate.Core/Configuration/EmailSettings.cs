namespace WebTemplate.Core.Configuration
{
    using System.ComponentModel.DataAnnotations;
    using WebTemplate.Core.Common;

    public class EmailSettings : IBaseConfiguration
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

        /// <summary>
        /// Validates email settings
        /// </summary>
        public Result Validate()
        {
            var errors = new List<Error>();

            if (string.IsNullOrWhiteSpace(Provider))
                errors.Add(Errors.Configuration.RequiredFieldMissing($"{SectionName}:Provider"));

            if (string.IsNullOrWhiteSpace(FromName))
                errors.Add(Errors.Configuration.RequiredFieldMissing($"{SectionName}:FromName"));

            // From, SmtpHost, etc. can be validated based on provider type if needed

            return errors.Any() ? Result.Failure(errors) : Result.Success();
        }
    }
}
