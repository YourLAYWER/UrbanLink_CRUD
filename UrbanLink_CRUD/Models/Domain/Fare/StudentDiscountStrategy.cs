namespace UrbanLink_CRUD.Models.Domain.Fare
{
    public class StudentDiscountStrategy : IFareStrategy
    {
        public decimal CalculateFare(FareCalculationContext context)
        {
            // Flat 30% discount for validated students
            if (context.UserProfile == "Student")
            {
                return context.BaseFare * 0.70m;
            }

            return context.BaseFare;
        }
    }
}
