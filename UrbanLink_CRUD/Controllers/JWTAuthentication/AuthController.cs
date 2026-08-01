using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Dtos.Authentication.Login;
using UrbanLink_CRUD.Models.Dtos.Authentication.Register;
using UrbanLink_CRUD.Models.Entities;

namespace UrbanLink_CRUD.Controllers.JWTAuthentication
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ULDBContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ULDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Registration Endpoint
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequestDto request)
        {
            // 1. Check if the email is already in use
            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Email is already registered." });
            }

            // 2. Hash the password securely using BCrypt
            // The workFactor (11) determines how computationally expensive the hash is. 
            // Higher = more secure, but slower to process. 11 is a standard sweet spot.
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 11);

            // 3. Create the new User entity from the DTO
            var newUser = new User
            {
                Name = request.FullName,
                Email = request.Email,
                Password = hashedPassword, // Store the hash, NEVER the plain text
                CommuterProfile = request.CommuterProfile,
                CreatedAt = DateTime.UtcNow
            };

            // 4. Save to the SQL database
            _context.Users.Add(newUser);
            _context.SaveChanges();

            // 5. Return a success response to the Android app
            return Ok(new { message = "User registered successfully." });
        }

        // Login Endpoint
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            // 1. Find the user in the database
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // 2. Verify the password
            // PRO TIP: In production, NEVER store plain text passwords. 
            // You would use BCrypt here: bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // 3. Create the Claims (The payload embedded inside the token)
            var claims = new List<Claim>
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.CommuterProfile) // e.g., "Student" or "Standard"
            };
            // 4. Generate the cryptographic signature
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            // 5. Assemble the Token
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15), // 15-minute lifespan for security
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // 6. Return the token to the Android app
            return Ok(new { token = tokenString });
        }
    }
}
