using System;
namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class RefundRequest
    {
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
