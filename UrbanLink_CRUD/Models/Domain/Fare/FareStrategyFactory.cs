namespace UrbanLink_CRUD.Models.Domain.Fare
{
    public class FareStrategyFactory : IFareStrategyFactory
    {
        public IFareStrategy GetStrategy(FareCalculationContext context)
        {
            if (context.UserProfile == "Student")
            {
                return new StudentDiscountStrategy();
            }

            int hour = context.TransitTime.Hour;
            if ((hour >= 7 && hour < 9) || (hour >= 16 && hour < 18))
            {
                return new PeakHourFareStrategy();
            }

            return new StandardFareStrategy();
        }
    }
}
