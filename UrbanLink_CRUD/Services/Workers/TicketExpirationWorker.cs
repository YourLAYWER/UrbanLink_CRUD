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

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ULDBContext>();

                        // Cut-off time (24 hours ago). Journeys are stored in local time by BookJourney.
                        var expirationThreshold = DateTime.Now.AddHours(-24);

                        // 1. Find the ticket references of journeys that are due to expire
                        var dueRefs = await context.Journeys
                            .Where(j => j.Status == JourneyStatus.Active && j.CreatedAt < expirationThreshold)
                            .Select(j => j.TicketReference.ToString())
                            .ToListAsync(stoppingToken);

                        if (dueRefs.Count > 0)
                        {
                            await using var tx = await context.Database.BeginTransactionAsync(stoppingToken);

                            // 2. Expire the matching Ticket rows (the admin Expired tab reads these)
                            int ticketsExpired = await context.Tickets
                                .Where(t => t.Status == TicketStatus.Active && dueRefs.Contains(t.QRCodeData))
                                .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, TicketStatus.Expired), stoppingToken);

                            // 3. Expire the journeys themselves
                            int journeysExpired = await context.Journeys
                                .Where(j => j.Status == JourneyStatus.Active && j.CreatedAt < expirationThreshold)
                                .ExecuteUpdateAsync(s => s.SetProperty(j => j.Status, JourneyStatus.Expired), stoppingToken);

                            await tx.CommitAsync(stoppingToken);

                            _logger.LogInformation(
                                "Auto-expired {Journeys} journeys and {Tickets} tickets.",
                                journeysExpired, ticketsExpired);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break; // app is shutting down
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "A fatal error occurred while processing ticket expirations.");
                }

                // Pause the loop for 1 hour before checking again
                try { await Task.Delay(TimeSpan.FromHours(1), stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }
}