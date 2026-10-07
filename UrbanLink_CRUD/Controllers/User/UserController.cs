using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Entities;

namespace UrbanLink_CRUD.Controllers.Users
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ULDBContext _context;

        public UserController(ULDBContext context)
        {
            _context = context;
        }

        // GET: api/user
        // Note: Password is deliberately excluded from every response below —
        // it's stored as a BCrypt hash, but there's no reason to ever send it
        // back over the network at all, hashed or not.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            return await _context.Users
                .Select(u => new { u.Id, u.Name, u.Email, u.Role, u.CreatedAt })
                .ToListAsync();
        }

        // GET: api/user/me
        //Gets the specific user details through the jwt credentials
        [HttpGet("me")]
        [Authorize] // This ensures only users with a valid JWT can access this
        public IActionResult GetUserProfile()
        {
            // 1. Extract the User ID securely from the JWT token
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null) return Unauthorized();

            int userId = int.Parse(userIdString);

            // 2. Fetch the user from the database
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound(new { message = "User not found" });

            // 3. Return the profile data
            return Ok(new
            {
                Name = user.Name,
                email = user.Email,
                commuterProfile = user.Role.ToString(),
                // You can easily add more fields here later, like WalletBalance or Role
            });
        }


        // GET: api/user/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = $"User with ID {id} not found." });

            return new { user.Id, user.Name, user.Email, user.Role, user.CreatedAt };
        }

        // PUT: api/user/5
        // Updates profile fields only — NOT password or email (those should go
        // through Auth-specific flows with proper verification, not a plain PUT).
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = $"User with ID {id} not found." });

            user.Name = updatedUser.Name;
            user.Role = updatedUser.Role;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/user/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = $"User with ID {id} not found." });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}