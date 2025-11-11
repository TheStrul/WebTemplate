namespace WebTemplate.Core.Services
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.DependencyInjection;
    using WebTemplate.Core.Interfaces;
    using WebTemplate.Core.Configuration.Features;

    public class RefreshTokenCleanupService : BackgroundService
    {
        private readonly ILogger<RefreshTokenCleanupService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval;

        public RefreshTokenCleanupService(
            ILogger<RefreshTokenCleanupService> logger,
            IServiceProvider serviceProvider,
            FeaturesOptions features
        )
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            var cleanupMinutes = features?.RefreshTokens?.CleanupIntervalMinutes ?? 360;
            _interval = TimeSpan.FromMinutes(cleanupMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
                    var cleaned = await tokenService.CleanupExpiredTokensAsync();
                    if (cleaned > 0)
                    {
                        _logger.LogInformation("RefreshTokenCleanupService removed {Count} expired tokens", cleaned);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during refresh token cleanup");
                }

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
            }
        }
    }
}
