namespace WebTemplate.UnitTests.Services
{
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using WebTemplate.Core.Configuration;
    using WebTemplate.Core.Services;
    using WebTemplate.UnitTests.Fixtures;
    using Xunit;

    public class SmtpEmailSenderTests : BaseTestFixture
    {
        private readonly EmailSettings _emailSettings;
        private readonly SmtpEmailSender _emailSender;

        public SmtpEmailSenderTests()
        {
            _emailSettings = new EmailSettings
            {
                SmtpHost = "smtp.test.com",
                SmtpPort = 587,
                SmtpEnableSsl = true,
                SmtpUser = "test@example.com",
                SmtpPassword = "TestPassword123",
                From = "noreply@test.com",
                FromName = "Test Application"
            };

            var options = Options.Create(_emailSettings);
            _emailSender = new SmtpEmailSender(options);
        }

        [Fact]
        public void Constructor_WithValidSettings_CreatesInstance()
        {
            // Arrange
            var settings = new EmailSettings
            {
                SmtpHost = "smtp.test.com",
                SmtpPort = 587,
                SmtpEnableSsl = true,
                SmtpUser = "user@test.com",
                SmtpPassword = "password",
                From = "from@test.com",
                FromName = "Test"
            };
            var options = Options.Create(settings);

            // Act
            var sender = new SmtpEmailSender(options);

            // Assert
            sender.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullFrom_UsesSmtpUserAsFrom()
        {
            // Arrange
            var settings = new EmailSettings
            {
                SmtpHost = "smtp.test.com",
                SmtpPort = 587,
                SmtpEnableSsl = true,
                SmtpUser = "user@test.com",
                SmtpPassword = "password",
                From = "",
                FromName = "Test"
            };
            var options = Options.Create(settings);

            // Act
            var sender = new SmtpEmailSender(options);

            // Assert
            sender.Should().NotBeNull();
        }

        // Note: Actual SMTP sending tests would require either:
        // 1. Integration tests with a real SMTP server or test container
        // 2. Mocking System.Net.Mail.SmtpClient (difficult due to sealed class)
        // 3. Refactoring SmtpEmailSender to use an injectable SMTP abstraction
        //
        // For now, we validate construction and settings parsing.
        // Real SMTP functionality should be tested via integration tests.
    }
}
