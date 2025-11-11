using System.ComponentModel.DataAnnotations;

namespace WebTemplate.Core.Configuration
{
    /// <summary>
    /// Feature flags for the User Module (configurable via JSON)
    /// </summary>
    public class UserModuleFeatures
    {
        public const string SectionName = "UserModule:Features";

        [Required]
        public bool EnableLogin { get; set; } = true;
        [Required]
        public bool EnableRegistration { get; set; } = true;
        [Required]
        public bool EnableRefreshToken { get; set; } = true;
        [Required]
        public bool EnableLogout { get; set; } = true;
        [Required]
        public bool EnableLogoutAllDevices { get; set; } = true;
        [Required]
        public bool EnableForgotPassword { get; set; } = true;
        [Required]
        public bool EnableResetPassword { get; set; } = true;
        [Required]
        public bool EnableConfirmEmail { get; set; } = true;
        [Required]
        public bool EnableChangePassword { get; set; } = true;

        /// <summary>
        /// Include UserType information and its Permissions in responses
        /// </summary>
        [Required]
        public bool IncludeUserTypePermissionsInResponses { get; set; } = true;
    }
}
