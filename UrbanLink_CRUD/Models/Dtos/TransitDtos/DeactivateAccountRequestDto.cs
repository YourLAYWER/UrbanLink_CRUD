using System;
/*namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{

}*/
namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class DeactivateAccountRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}

