namespace UrbanLink_CRUD.Models.Domain.Incident
{
    public class IncidentStrategyResolver
    {
        private readonly IEnumerable<IIncidentStrategy> _strategies;

        public IncidentStrategyResolver(IEnumerable<IIncidentStrategy> strategies)
        {
            _strategies = strategies;
        }

        public IIncidentStrategy Resolve(int categoryId)
        {
            var strategy = _strategies.FirstOrDefault(s => s.CategoryId == categoryId);

            if (strategy == null)
            {
                throw new NotSupportedException($"Category ID {categoryId} is not supported.");
            }

            return strategy;
        }
    }
}
