public class CreateJourneyLegDto
{
    public int JourneyId { get; set; }
    public int SequenceOrder { get; set; }
    public int RouteId { get; set; }
    public int BoardingStopId { get; set; }
    public int DestinationStopId { get; set; }
    public decimal LegFare { get; set; }
    public int LegDurationMinutes { get; set; }
}