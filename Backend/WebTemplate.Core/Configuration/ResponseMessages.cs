namespace WebTemplate.Core.Configuration
{
    /// <summary>
    /// Centralized configurable response messages used by API controllers.
    /// Override values via configuration section "ResponseMessages".
    /// </summary>
    public class ResponseMessages
    {
        public AuthResponseMessages Auth { get; set; } = new();
    }

    public class AuthResponseMessages
    {
        public string InvalidInput { get; set; } = "Invalid input data";
        public string RegistrationInvalid { get; set; } = "Invalid registration data";
        public string Unauthorized { get; set; } = "Unauthorized";

        public string TryAgainLater { get; set; } = "Please try again later";

        public string LoginInternalError { get; set; } = "An internal error occurred during login";
        public string RegistrationInternalError { get; set; } = "An internal error occurred during registration";
        public string TokenRefreshError { get; set; } = "An error occurred during token refresh";
        public string LogoutError { get; set; } = "An error occurred during logout";
        public string GenericError { get; set; } = "An error occurred";
        public string PasswordResetError { get; set; } = "An error occurred during password reset";
        public string ChangePasswordError { get; set; } = "An error occurred during change password";
        public string EmailConfirmationError { get; set; } = "An error occurred during email confirmation";
    }
}
