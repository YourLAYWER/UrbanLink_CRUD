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
        // POST: api/admin/broadcast
        // Sends a system alert notification to all registered users
        [HttpPost]
        public async Task<IActionResult> SendBroadcast(BroadcastDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest(new { message = "Broadcast message cannot be empty." });

            // Automatically append the current time to the message string
            string stampedMessage = $"{dto.Message.Trim()} (Sent: {DateTime.Now:dd MMM, hh:mm tt})";

            var targetUserIds = await _context.Users.Select(u => u.Id).ToListAsync();

            if (!targetUserIds.Any())
                return Ok(new { message = "No registered users found. No notifications sent.", recipientCount = 0 });

            var notifications = targetUserIds.Select(userId => new Notification
            {
                Type = "broadcast_alert",
                Message = stampedMessage, // Save the time-stamped version
                UserId = userId
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Broadcast sent.", recipientCount = notifications.Count });
        }

        // GET: api/admin/broadcast/history
        [HttpGet("history")]
        public async Task<IActionResult> GetBroadcastHistory()
        {
            // 1. Fetch the records from the database first to avoid SQL translation errors
            var allBroadcasts = await _context.Notifications
                .Where(n => n.Type == "broadcast_alert")
                .OrderByDescending(n => n.Id)
                .ToListAsync();

            // 2. Safely group them in memory
            var history = allBroadcasts
                .GroupBy(n => n.Message)
                .Select(g => new
                {
                    Id = g.First().Id, // Use the ID of the first one in the group
                    Message = g.Key,
                    RecipientCount = g.Count()
                })
                .Take(10) // Keep the UI clean by only showing the last 10
                .ToList();

            return Ok(history);
        }
    }
}