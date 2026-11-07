using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Domain.SignalR;
using UrbanLink_CRUD.Models.Entities.Transit;

namespace UrbanLink_CRUD.Services.Workers
{
    public class LocationAuditBatcher : BackgroundService
    {
        private readonly TelemetryQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LocationAuditBatcher> _logger;

        public LocationAuditBatcher(TelemetryQueue queue, IServiceProvider serviceProvider, ILogger<LocationAuditBatcher> logger)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Wait 5 minutes (Change to 30 seconds for testing today!)
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

                var batch = _queue.DequeueAll();
                if (batch.Any())
                {
                    _logger.LogInformation($"Batching {batch.Count} GPS records to SQL Server...");

                    // We must create a new scope because DbContext is Scoped, but BackgroundService is Singleton
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ULDBContext>();

                    // Map DTOs to your actual Database Entities (e.g., BusTelemetry)
                    var entities = batch.Select(dto => new BusTelemetry
                    {
                        BusId = dto.BusId,
                        RouteId = dto.RouteId,
                        Latitude = dto.Latitude,
                        Longitude = dto.Longitude,
                        Timestamp = DateTime.UtcNow
                    });

                    await dbContext.BusTelemetries.AddRangeAsync(entities, stoppingToken);
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
        }
    }
}