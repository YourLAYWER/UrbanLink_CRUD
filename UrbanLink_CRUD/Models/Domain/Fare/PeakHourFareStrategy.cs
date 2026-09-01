namespace UrbanLink_CRUD.Models.Domain.Fare
{
    public class PeakHourFareStrategy : IFareStrategy
    {
        public decimal CalculateFare(FareCalculationContext context)
        {
            // Example: 20% surcharge between 07:00-09:00 and 16:00-18:00
            int hour = context.TransitTime.Hour;
            bool isMorningPeak = hour >= 7 && hour < 9;
            bool isEveningPeak = hour >= 16 && hour < 18;

            if (isMorningPeak || isEveningPeak)
            {
                return context.BaseFare * 1.20m;
            }

            return context.BaseFare;
        }
    }
}
