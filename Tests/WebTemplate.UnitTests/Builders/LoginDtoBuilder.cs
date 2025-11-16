namespace WebTemplate.UnitTests.Builders
{
    using WebTemplate.Core.DTOs.Auth;

    /// <summary>
    /// Builder for creating LoginDto test data
    /// </summary>
    public class LoginDtoBuilder
    {
        private string _email = "test@example.com";
        private string _password = "Test123!";

        public LoginDtoBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public LoginDtoBuilder WithPassword(string password)
        {
            _password = password;
            return this;
        }

        public LoginDtoBuilder WithInvalidEmail()
        {
            _email = "invalid-email";
            return this;
        }

        public LoginDtoBuilder WithWeakPassword()
        {
            _password = "weak";
            return this;
        }

        public LoginDto Build() => new()
        {
            Email = _email,
            Password = _password
        };
    }
}
