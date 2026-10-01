using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Dtos.Authentication.Login;
using UrbanLink_CRUD.Models.Dtos.Authentication.Register;
using UrbanLink_CRUD.Models.Entities;
using UrbanLink_CRUD.Models.Enums;
using System.Security.Cryptography;

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

        // Registration Endpoint for standard commuter
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
                Role = Models.Enums.Role.StandardUser,
                CreatedAt = DateTime.UtcNow
            };

            // 4. Save to the SQL database
            _context.Users.Add(newUser);
            _context.SaveChanges();

            // 5. Return a success response to the Android app
            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("register-staff")]
        // [Authorize(Roles = "Admin")] // Uncomment this once you have an Admin user to test with!
        public IActionResult RegisterStaff([FromBody] RegisterStaffRequestDto request)
        {
            // 1. Check if the email is already taken
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email is already in use." });
            }

            // 2. Validate the role to prevent typos or unauthorized roles
            var validStaffRoles = new List<string> { "Conductor", "Driver", "Admin" };
            if (!validStaffRoles.Contains(request.Role))
            {
                return BadRequest(new { message = "Invalid role. Must be Conductor, Driver, or Admin." });
            }

            // 3. Hash the password and create the user
            var newUser = new User // Change 'User' if your entity is named differently (e.g., ApplicationUser)
            {
                Name = request.Name,
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),

                // If your User.Role is a string, do this:
                //Role = request.Role

                // If your User.Role is an Enum, do this instead:
                Role = Enum.Parse<Role>(request.Role) 
            };

            // 4. Save to the database
            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(new { message = $"{request.Role} account created successfully." });
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
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // 3. Create the Claims for the Access Token
            var claims = new List<Claim>
    {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
    };

            // 4. Generate the cryptographic signature for the JWT
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            // 5. Assemble the Short-Lived Access Token (15 Minutes)
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15), // short lifespan
                signingCredentials: credentials);

            var accessTokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // 6. Generate the Long-Lived Refresh Token (7 Days)
            var refreshTokenString = GenerateRefreshToken();

            // 7. Save the Refresh Token to the Database
            user.RefreshToken = refreshTokenString;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            _context.SaveChanges();

            // 8. Return BOTH tokens to the Android app
            return Ok(new
            {
                token = accessTokenString,         // The short-lived JWT
                refreshToken = refreshTokenString, // The long-lived refresh token
                role = user.Role.ToString()
            });
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequestDto request)
        {
            // 1. Find the user by their unique refresh token
            var user = _context.Users.FirstOrDefault(u => u.RefreshToken == request.RefreshToken);

            // If no user has this token, or it was tampered with, reject it
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid refresh token." });
            }

            // 2. Check if the refresh token has expired (e.g., it's been more than 7 days)
            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                // The token is dead. Erase it from the DB to be safe and force a hard login.
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                _context.SaveChanges();

                return Unauthorized(new { message = "Refresh token expired. Please log in again." });
            }

            // 3. The token is valid! Generate a brand new Access Token (JWT)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials);

            var newAccessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // 4. Token Rotation: Generate a new Refresh Token to replace the old one.
            // This is a massive security boost. If a hacker steals a refresh token, 
            // it becomes useless the moment the real user refreshes their app.
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Reset the 7-day clock
            _context.SaveChanges();

            // 5. Return the brand new pair to the Android app
            return Ok(new
            {
                token = newAccessToken,
                refreshToken = newRefreshToken,
                role = user.Role.ToString()
            });
        }

        // The method below generates a higly secure 64-byte random string
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
