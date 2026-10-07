namespace UrbanLink_CRUD.Models.Dtos.Authentication.Login
{
    public class RefreshRequestDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
