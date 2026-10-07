namespace UrbanLink_CRUD.Services.RoutingLogic
{
    public class RoutingStrategyFactory : IRoutingStrategyFactory
    {
        public IRoutingStrategy GetStrategy(string preference)
        {
            return (preference?.ToLower()) switch
            {
                //"cheapest" => new CheapestRouteStrategy(),
                //"least_transfers" => new LeastTransfersStrategy(),
                _ => new FastestRouteStrategy()
            };
        }
    }
}
