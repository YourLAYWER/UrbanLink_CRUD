using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Entities;
using UrbanLink_CRUD.Models.Entities.Transit;
using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")]
    public class BroadcastController : ControllerBase
    {
        private readonly ULDBContext _context;

        public BroadcastController(ULDBContext context)
        {
            _context = context;
        }

        public class BroadcastDto
        {
            public string Message { get; set; } = string.Empty;

            // Optional parameter passed by frontend
            public int? RouteSegmentID { get; set; }
        }

        // POST: api/admin/broadcast
        // Sends a system alert notification to all registered users
        [HttpPost]
        public async Task<IActionResult> SendBroadcast(BroadcastDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest(new { message = "Broadcast message cannot be empty." });

            var targetUserIds = await _context.Users.Select(u => u.Id).ToListAsync();

            if (!targetUserIds.Any())
                return Ok(new { message = "No registered users found. No notifications sent.", recipientCount = 0 });

            var notifications = targetUserIds.Select(userId => new Notification
            {
                Type = "broadcast_alert",
                Message = dto.Message,
                UserId = userId
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Broadcast sent.", recipientCount = notifications.Count });
        }
    }
}