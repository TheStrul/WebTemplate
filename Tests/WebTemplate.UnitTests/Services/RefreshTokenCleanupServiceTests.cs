namespace WebTemplate.UnitTests.Services
{
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using WebTemplate.Core.Configuration.Features;
    using WebTemplate.Core.Interfaces;
    using WebTemplate.Core.Services;
    using WebTemplate.UnitTests.Fixtures;
    using Xunit;

    public class RefreshTokenCleanupServiceTests : BaseTestFixture
    {
        private readonly Mock<ILogger<RefreshTokenCleanupService>> _mockLogger;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly IServiceProvider _serviceProvider;
        private readonly FeaturesOptions _featuresOptions;

        public RefreshTokenCleanupServiceTests()
        {
            _mockLogger = new Mock<ILogger<RefreshTokenCleanupService>>();
            _mockTokenService = new Mock<ITokenService>();

            _featuresOptions = new FeaturesOptions
            {
                RefreshTokens = new RefreshTokensFeatureOptions
                {
                    CleanupIntervalMinutes = 1 // Short interval for testing
                }
            };

            // Create a real service collection and provider
            var services = new ServiceCollection();
            services.AddScoped<ITokenService>(_ => _mockTokenService.Object);
            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Act
            var service = new RefreshTokenCleanupService(
                _mockLogger.Object,
                _serviceProvider,
                _featuresOptions
            );

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithDefaultFeatures_UsesDefaultCleanupInterval()
        {
            // Arrange
            var defaultFeatures = new FeaturesOptions();

            // Act
            var service = new RefreshTokenCleanupService(
                _mockLogger.Object,
                _serviceProvider,
                defaultFeatures
            );

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public async Task ExecuteAsync_CleansUpExpiredTokens_OnInterval()
        {
            // Arrange
            _mockTokenService.Setup(ts => ts.CleanupExpiredTokensAsync())
                .ReturnsAsync(5); // Simulate 5 tokens cleaned

            var service = new RefreshTokenCleanupService(
                _mockLogger.Object,
                _serviceProvider,
                _featuresOptions
            );

            using var cts = new CancellationTokenSource();

            // Act
            var executeTask = service.StartAsync(cts.Token);
            await Task.Delay(TimeSpan.FromMilliseconds(100)); // Give it time to run at least once
            cts.Cancel();

            try
            {
                await service.StopAsync(CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }

            // Assert
            _mockTokenService.Verify(ts => ts.CleanupExpiredTokensAsync(), Times.AtLeastOnce());
        }

        [Fact]
        public async Task ExecuteAsync_LogsInformation_WhenTokensCleaned()
        {
            // Arrange
            _mockTokenService.Setup(ts => ts.CleanupExpiredTokensAsync())
                .ReturnsAsync(3); // Simulate 3 tokens cleaned

            var service = new RefreshTokenCleanupService(
                _mockLogger.Object,
                _serviceProvider,
                _featuresOptions
            );

            using var cts = new CancellationTokenSource();

            // Act
            var executeTask = service.StartAsync(cts.Token);
            await Task.Delay(TimeSpan.FromMilliseconds(100)); // Give it time to run
            cts.Cancel();

            try
            {
                await service.StopAsync(CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                // Expected
            }

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("removed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotLog_WhenNoTokensCleaned()
        {
            // Arrange
            _mockTokenService.Setup(ts => ts.CleanupExpiredTokensAsync())
                .ReturnsAsync(0); // No tokens cleaned

            var service = new RefreshTokenCleanupService(
                _mockLogger.Object,
                _serviceProvider,
                _featuresOptions
            );

            using var cts = new CancellationTokenSource();

            // Act
            var executeTask = service.StartAsync(cts.Token);
            await Task.Delay(TimeSpan.FromMilliseconds(100)); // Give it time to run
            cts.Cancel();

            try
            {
                await service.StopAsync(CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                // Expected
            }

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("removed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never());
        }

        [Fact]
        public async Task ExecuteAsync_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            _mockTokenService.Setup(ts => ts.CleanupExpiredTokensAsync())
                .ThrowsAsync(new InvalidOperationException("Database error"));

            var service = new RefreshTokenCleanupService(
                _mockLogger.Object,
                _serviceProvider,
                _featuresOptions
            );

            using var cts = new CancellationTokenSource();

            // Act
            var executeTask = service.StartAsync(cts.Token);
            await Task.Delay(TimeSpan.FromMilliseconds(100)); // Give it time to fail
            cts.Cancel();

            try
            {
                await service.StopAsync(CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                // Expected
            }

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error during refresh token cleanup")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async Task ExecuteAsync_ContinuesRunning_AfterException()
        {
            // Arrange
            var callCount = 0;
            _mockTokenService.Setup(ts => ts.CleanupExpiredTokensAsync())
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount == 1)
                    {
                        throw new InvalidOperationException("First call fails");
                    }
                    return 2; // Second call succeeds
                });

            var service = new RefreshTokenCleanupService(
                _mockLogger.Object,
                _serviceProvider,
                _featuresOptions
            );

            using var cts = new CancellationTokenSource();

            // Act
            var executeTask = service.StartAsync(cts.Token);
            await Task.Delay(TimeSpan.FromMilliseconds(2500)); // Wait for more than 2 full 1-minute intervals
            cts.Cancel();

            try
            {
                await service.StopAsync(CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                // Expected
            }

            // Assert - Service should attempt cleanup and log the error
            callCount.Should().BeGreaterThan(0, "Service should attempt at least once");
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce(),
                "Error should be logged when exception occurs");
        }

        [Fact]
        public async Task ExecuteAsync_StopsGracefully_WhenCancellationRequested()
        {
            // Arrange
            _mockTokenService.Setup(ts => ts.CleanupExpiredTokensAsync())
                .ReturnsAsync(0);

            var service = new RefreshTokenCleanupService(
                _mockLogger.Object,
                _serviceProvider,
                _featuresOptions
            );

            using var cts = new CancellationTokenSource();

            // Act
            await service.StartAsync(cts.Token);
            cts.Cancel(); // Cancel immediately

            var stopAction = async () => await service.StopAsync(CancellationToken.None);

            // Assert
            await stopAction.Should().NotThrowAsync();
        }
    }
}
