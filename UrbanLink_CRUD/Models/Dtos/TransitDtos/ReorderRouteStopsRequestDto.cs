namespace UrbanLink_CRUD.Models.Dtos.Transit
{
    public class ReorderRouteStopsRequestDto
    {
        // Stop IDs in the new travel order for this segment. Must be exactly
        // the same set of stop IDs already on the segment, just reordered.
        public List<int> OrderedStopIds { get; set; } = new();
    }
}
