namespace WebTemplate.Core.Configuration
{
    using WebTemplate.Core.Common;

    /// <summary>
    /// Application public URLs for building links in emails, etc.
    /// </summary>
    public class AppUrls
    {
        public const string SectionName = "AppUrls";

        /// <summary>
        /// Base URL of the frontend (e.g., https://localhost:5173)
        /// </summary>
        public string FrontendBaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Relative path of the confirm email page on the frontend (e.g., /confirm-email)
        /// </summary>
        public string ConfirmEmailPath { get; set; } = "/confirm-email";

        /// <summary>
        /// Validates AppUrls settings
        /// </summary>
        public Result Validate()
        {
            var errors = new List<Error>();

            // FrontendBaseUrl can be empty in some scenarios, but validate format if present
            if (!string.IsNullOrWhiteSpace(FrontendBaseUrl) &&
                !Uri.TryCreate(FrontendBaseUrl, UriKind.Absolute, out _))
            {
                errors.Add(Errors.Configuration.InvalidFormat(
                    "AuthSettings:AppUrls:FrontendBaseUrl",
                    $"'{FrontendBaseUrl}' is not a valid absolute URL."
                ));
            }

            if (string.IsNullOrWhiteSpace(ConfirmEmailPath))
                errors.Add(Errors.Configuration.RequiredFieldMissing("AuthSettings:AppUrls:ConfirmEmailPath"));

            return errors.Any() ? Result.Failure(errors) : Result.Success();
        }
    }
}
