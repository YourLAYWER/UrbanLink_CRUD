namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class BookTicketRequest
    {
        public int StartStopId { get; set; }
        public int EndStopId { get; set; }
        public decimal FinalAmountPaid { get; set; } // Accounts for the student discount
        public bool IsStudentPassApplied { get; set; }
        // In a production app, you would also pass a PaymentToken here
    }
}
