using System.ComponentModel.DataAnnotations;
using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Models.Dtos.Transit
{
    public class UpdateRouteRequestDto
    {
        [Required, StringLength(100, MinimumLength = 1)]
        public string RouteName { get; set; } = string.Empty;

        [EnumDataType(typeof(TransitType))]
        public TransitType TransportType { get; set; }

        [Range(0, 10000)]
        public decimal BaseFare { get; set; }

        public int ProviderId { get; set; }
    }
}
