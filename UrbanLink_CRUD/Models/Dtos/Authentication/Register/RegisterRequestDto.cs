namespace UrbanLink_CRUD.Models.Dtos.Authentication.Register
{
    public class RegisterRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string CommuterProfile { get; set; } = "Standard"; // Default profile is "Standard"
    }
}
