namespace UrbanLink_CRUD.Services.RoutingLogic
{
    public interface IRoutingStrategyFactory
    {
        IRoutingStrategy GetStrategy(string preference);
    }
}
