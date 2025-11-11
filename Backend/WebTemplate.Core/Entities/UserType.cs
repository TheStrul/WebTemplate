using System.ComponentModel.DataAnnotations;

namespace WebTemplate.Core.Entities
{
    public class UserType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        // User type permissions (JSON stored as string for flexibility)
        [StringLength(2000)]
        public string? Permissions { get; set; }
    }
}