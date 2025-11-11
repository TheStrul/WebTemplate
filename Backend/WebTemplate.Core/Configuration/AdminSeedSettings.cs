namespace WebTemplate.Core.Configuration
{
    public class AdminSeedSettings
    {
        public const string SectionName = "AdminSeed";
        public bool Enabled { get; set; } = false;
        public string Email { get; set; } = "admin@WebTemplate.com";
        public string Password { get; set; } = "Admin123!";
        public string FirstName { get; set; } = "System";
        public string LastName { get; set; } = "Administrator";
    }
}
