namespace UrbanLink_CRUD.Models.Domain.Fare
{
    public interface IFareStrategy
    {
        decimal CalculateFare(FareCalculationContext context);
    }
}
