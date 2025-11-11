namespace WebTemplate.Core.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Refresh token entity for JWT token management
    /// Stores refresh tokens with expiration and device tracking
    /// </summary>
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Token { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }

        [StringLength(100)]
        public string? DeviceId { get; set; }

        [StringLength(100)]
        public string? DeviceName { get; set; }

        [StringLength(45)] // IPv6 max length
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Whether this refresh token is still active
        /// </summary>
        [NotMapped]
        public bool IsActive => RevokedAt == null && DateTime.UtcNow <= ExpiryDate;

        /// <summary>
        /// Whether this refresh token has expired
        /// </summary>
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow > ExpiryDate;

        /// <summary>
        /// Whether this refresh token has been revoked
        /// </summary>
        [NotMapped]
        public bool IsRevoked => RevokedAt != null;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Revoke this refresh token
        /// </summary>
        public void Revoke()
        {
            RevokedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Check if token should be cleaned up (expired or revoked)
        /// </summary>
        /// <returns>True if token should be deleted</returns>
        public bool ShouldCleanup()
        {
            return IsExpired || IsRevoked;
        }
    }
}