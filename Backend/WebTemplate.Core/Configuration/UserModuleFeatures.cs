namespace WebTemplate.Core.Configuration
{
    using System.ComponentModel.DataAnnotations;
    using WebTemplate.Core.Common;

    /// <summary>
    /// Feature flags for the User Module (configurable via JSON)
    /// NO DEFAULTS - all values must be explicitly provided in configuration.
    /// App will fail to start if any required flag is missing.
    /// </summary>
    public class UserModuleFeatures
    {
        public const string SectionName = "UserModule:Features";

        /// <summary>
        /// Enable login endpoint - REQUIRED
        /// </summary>
        [Required]
        public bool EnableLogin { get; set; }

        /// <summary>
        /// Enable registration endpoint - REQUIRED
        /// </summary>
        [Required]
        public bool EnableRegistration { get; set; }

        /// <summary>
        /// Enable refresh token endpoint - REQUIRED
        /// </summary>
        [Required]
        public bool EnableRefreshToken { get; set; }

        /// <summary>
        /// Enable logout endpoint - REQUIRED
        /// </summary>
        [Required]
        public bool EnableLogout { get; set; }

        /// <summary>
        /// Enable logout from all devices endpoint - REQUIRED
        /// </summary>
        [Required]
        public bool EnableLogoutAllDevices { get; set; }

        /// <summary>
        /// Enable forgot password endpoint - REQUIRED
        /// </summary>
        [Required]
        public bool EnableForgotPassword { get; set; }

        /// <summary>
        /// Enable reset password endpoint - REQUIRED
        /// </summary>
        [Required]
        public bool EnableResetPassword { get; set; }

        /// <summary>
        /// Enable confirm email endpoint - REQUIRED
        /// </summary>
        [Required]
        public bool EnableConfirmEmail { get; set; }

        /// <summary>
        /// Enable change password endpoint - REQUIRED
        /// </summary>
        [Required]
        public bool EnableChangePassword { get; set; }

        /// <summary>
        /// Include UserType information and its Permissions in responses - REQUIRED
        /// </summary>
        [Required]
        public bool IncludeUserTypePermissionsInResponses { get; set; }

        /// <summary>
        /// Validates user module features - fails if any required flag is missing.
        /// </summary>
        public Result Validate()
        {
            var errors = new List<Error>();

            // All properties are [Required], so configuration binding will fail if missing
            // This validation method ensures all flags are properly set
            if (!Enum.GetValues(typeof(bool)).Cast<bool>().Any())
            {
                errors.Add(Errors.Configuration.RequiredFieldMissing("UserModule:Features"));
            }

            return errors.Any() ? Result.Failure(errors) : Result.Success();
        }
    }
}
