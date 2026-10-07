using UrbanLink_CRUD.Models.Dtos.TransitDtos;

namespace UrbanLink_CRUD.Models.Domain.Incident
{
    public interface IIncidentStrategy
    {
        // Matches the integer from your Android app (e.g., 5 for Delay, 6 for Medical)
        int CategoryId { get; }

        Task ProcessIncidentAsync(IncidentReportRequestDto request, int routeId);
    }
}
