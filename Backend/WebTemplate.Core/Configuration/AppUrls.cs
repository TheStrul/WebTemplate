namespace WebTemplate.Core.Configuration
{
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
    }
}
