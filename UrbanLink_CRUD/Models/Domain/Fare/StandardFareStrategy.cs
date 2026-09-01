namespace UrbanLink_CRUD.Models.Domain.Fare
{
    public class StandardFareStrategy : IFareStrategy
    {
        public decimal CalculateFare(FareCalculationContext context)
        {
            return context.BaseFare;
        }
    }
}
