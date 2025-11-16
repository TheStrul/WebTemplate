namespace WebTemplate.UnitTests.Builders
{
    using WebTemplate.Core.Entities;

    /// <summary>
    /// Builder for creating ApplicationUser test data
    /// </summary>
    public class ApplicationUserBuilder
    {
        private string _id = Guid.NewGuid().ToString();
        private string _email = "test@example.com";
        private string _userName = "test@example.com";
        private string _firstName = "Test";
        private string _lastName = "User";
        private bool _emailConfirmed = true;
        private bool _isActive = true;
        private int _userTypeId = 2; // Default to "User" type
        private string _passwordHash = "hashed-password";

        public ApplicationUserBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        public ApplicationUserBuilder WithEmail(string email)
        {
            _email = email;
            _userName = email; // Keep username synced
            return this;
        }

        public ApplicationUserBuilder WithFirstName(string firstName)
        {
            _firstName = firstName;
            return this;
        }

        public ApplicationUserBuilder WithLastName(string lastName)
        {
            _lastName = lastName;
            return this;
        }

        public ApplicationUserBuilder WithEmailConfirmed(bool confirmed)
        {
            _emailConfirmed = confirmed;
            return this;
        }

        public ApplicationUserBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public ApplicationUserBuilder WithUserTypeId(int userTypeId)
        {
            _userTypeId = userTypeId;
            return this;
        }

        public ApplicationUserBuilder WithPasswordHash(string passwordHash)
        {
            _passwordHash = passwordHash;
            return this;
        }

        public ApplicationUserBuilder AsInactive()
        {
            _isActive = false;
            return this;
        }

        public ApplicationUserBuilder AsUnconfirmed()
        {
            _emailConfirmed = false;
            return this;
        }

        public ApplicationUserBuilder AsAdmin()
        {
            _userTypeId = 1;
            return this;
        }

        public ApplicationUser Build() => new()
        {
            Id = _id,
            Email = _email,
            UserName = _userName,
            NormalizedEmail = _email.ToUpperInvariant(),
            NormalizedUserName = _userName.ToUpperInvariant(),
            FirstName = _firstName,
            LastName = _lastName,
            EmailConfirmed = _emailConfirmed,
            IsActive = _isActive,
            UserTypeId = _userTypeId,
            PasswordHash = _passwordHash,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
    }
}
