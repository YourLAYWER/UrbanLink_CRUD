using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                .Select(u => new { u.Id, u.Name, u.Email, u.CommuterProfile, u.CreatedAt })
                .ToListAsync();
        }

        // GET: api/user/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = $"User with ID {id} not found." });

            return new { user.Id, user.Name, user.Email, user.CommuterProfile, user.CreatedAt };
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
            user.CommuterProfile = updatedUser.CommuterProfile;

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