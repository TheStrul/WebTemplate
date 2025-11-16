namespace WebTemplate.UnitTests.Builders
{
    using WebTemplate.Core.DTOs.Auth;

    /// <summary>
    /// Builder for creating RegisterDto test data
    /// </summary>
    public class RegisterDtoBuilder
    {
        private string _email = "test@example.com";
        private string _password = "Test123!";
        private string _confirmPassword = "Test123!";
        private string _firstName = "Test";
        private string _lastName = "User";

        public RegisterDtoBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public RegisterDtoBuilder WithPassword(string password)
        {
            _password = password;
            return this;
        }

        public RegisterDtoBuilder WithConfirmPassword(string confirmPassword)
        {
            _confirmPassword = confirmPassword;
            return this;
        }

        public RegisterDtoBuilder WithMismatchedPasswords()
        {
            _confirmPassword = "Different123!";
            return this;
        }

        public RegisterDtoBuilder WithFirstName(string firstName)
        {
            _firstName = firstName;
            return this;
        }

        public RegisterDtoBuilder WithLastName(string lastName)
        {
            _lastName = lastName;
            return this;
        }

        public RegisterDtoBuilder WithInvalidEmail()
        {
            _email = "invalid-email";
            return this;
        }

        public RegisterDtoBuilder WithWeakPassword()
        {
            _password = "weak";
            _confirmPassword = "weak";
            return this;
        }

        public RegisterDto Build() => new()
        {
            Email = _email,
            Password = _password,
            ConfirmPassword = _confirmPassword,
            FirstName = _firstName,
            LastName = _lastName
        };
    }
}
