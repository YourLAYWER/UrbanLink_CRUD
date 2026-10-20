
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Services.Workers
{
    public class TicketExpirationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TicketExpirationWorker> _logger;

        public TicketExpirationWorker(IServiceProvider serviceProvider, ILogger<TicketExpirationWorker> logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Ticket Expiration Worker started.");

            // Keep running until the application shuts down
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1. Create a fresh scope to safely inject the DbContext
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ULDBContext>();

                        // 2. Define the cut-off time (24 hours ago)
                        var expirationThreshold = DateTime.Now.AddHours(-24);

                        // 3. Bulk Update: Instantly change the status using the strong Enum
                        int expiredCount = await context.Journeys
                            .Where(j => j.Status == JourneyStatus.Active && j.CreatedAt < expirationThreshold)
                            .ExecuteUpdateAsync(s => s.SetProperty(j => j.Status, JourneyStatus.Expired), stoppingToken);

                        if (expiredCount > 0)
                        {
                            _logger.LogInformation($"Auto-expired {expiredCount} unused tickets.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "A fatal error occurred while processing ticket expirations.");
                }

                // 4. Pause the loop for 1 hour before checking again
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
