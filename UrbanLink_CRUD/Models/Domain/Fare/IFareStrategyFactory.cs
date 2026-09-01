namespace UrbanLink_CRUD.Models.Domain.Fare
{
    public interface IFareStrategyFactory
    {
        IFareStrategy GetStrategy(FareCalculationContext context);
    }
}
