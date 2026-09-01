using System;


namespace UrbanLink_CRUD.Models.Domain.Fare
{
    public record FareCalculationContext(
        decimal BaseFare,
        DateTime TransitTime,
        string UserProfile // e.g., "Student", "Standard", "Pensioner"
    );
}
