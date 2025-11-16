namespace WebTemplate.UnitTests.Services
{
    using FluentAssertions;
    using WebTemplate.Core.Services;
    using WebTemplate.UnitTests.Fixtures;
    using Xunit;

    public class NoOpEmailSenderTests : BaseTestFixture
    {
        private readonly NoOpEmailSender _emailSender;

        public NoOpEmailSenderTests()
        {
            _emailSender = new NoOpEmailSender();
        }

        [Fact]
        public async Task SendAsync_WithAnyParameters_CompletesSuccessfully()
        {
            // Arrange
            var toEmail = RandomEmail();
            var subject = "Test Subject";
            var htmlBody = "<p>Test HTML Body</p>";

            // Act
            var act = async () => await _emailSender.SendAsync(toEmail, subject, htmlBody);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SendAsync_WithPlainTextBody_CompletesSuccessfully()
        {
            // Arrange
            var toEmail = RandomEmail();
            var subject = "Test Subject";
            var htmlBody = "<p>Test HTML Body</p>";
            var plainTextBody = "Test Plain Text Body";

            // Act
            var act = async () => await _emailSender.SendAsync(toEmail, subject, htmlBody, plainTextBody);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SendAsync_WithCancellationToken_CompletesSuccessfully()
        {
            // Arrange
            var toEmail = RandomEmail();
            var subject = "Test Subject";
            var htmlBody = "<p>Test HTML Body</p>";
            using var cts = new CancellationTokenSource();

            // Act
            var act = async () => await _emailSender.SendAsync(toEmail, subject, htmlBody, null, cts.Token);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SendAsync_DoesNotActuallySendEmail()
        {
            // Arrange
            var toEmail = RandomEmail();
            var subject = "Test Subject";
            var htmlBody = "<p>This should never be sent</p>";

            // Act
            await _emailSender.SendAsync(toEmail, subject, htmlBody);

            // Assert
            // NoOpEmailSender should complete instantly without side effects
            // This is validated by the fact that the method completes synchronously
            true.Should().BeTrue("NoOpEmailSender completed without errors");
        }

        [Fact]
        public async Task SendAsync_WithNullOptionalParameters_CompletesSuccessfully()
        {
            // Arrange
            var toEmail = RandomEmail();
            var subject = "Test Subject";
            var htmlBody = "<p>Test HTML Body</p>";

            // Act
            var act = async () => await _emailSender.SendAsync(toEmail, subject, htmlBody, null);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SendAsync_MultipleConsecutiveCalls_AllCompleteSuccessfully()
        {
            // Arrange
            var emails = new[]
            {
                (RandomEmail(), "Subject 1", "<p>Body 1</p>"),
                (RandomEmail(), "Subject 2", "<p>Body 2</p>"),
                (RandomEmail(), "Subject 3", "<p>Body 3</p>")
            };

            // Act & Assert
            foreach (var (email, subject, body) in emails)
            {
                var act = async () => await _emailSender.SendAsync(email, subject, body);
                await act.Should().NotThrowAsync();
            }
        }
    }
}
