using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using UrbanLink_CRUD.Models.Entities; // 👈 Matches your User.cs namespace
using UrbanLink_CRUD.Models.Data;     // 👈 Matches your ULDBContext namespace

namespace UrbanLink_CRUD.Controllers.TransitControllers
{
    [ApiController]
    [Route("api")]
    [AllowAnonymous]
    public class AdminController : ControllerBase
    {
        // 1. Account Management (Connected to EF Core Database)
        [HttpPost("admin/accounts/create")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest dto, [FromServices] ULDBContext context)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.FullName))
            {
                return BadRequest(new { message = "Full name and email are required." });
            }

            // Check if user already exists in SQL Server
            var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "An account with this email already exists." });
            }

            // Securely hash temporary password using BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword("UrbanLink@123");

            // Note: If your User model's 'Role' property expects an Enum instead of a string, 
            // you can parse it or map it safely like this:
            Enum.TryParse<UrbanLink_CRUD.Models.Enums.Role>(dto.Role, true, out var parsedRole);

            var newUser = new User
            {
                Name = dto.FullName,
                Email = dto.Email,
                Password = hashedPassword,
                Role = parsedRole,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(newUser);
            await context.SaveChangesAsync();

            return Ok(new { message = $"Account for {dto.FullName} created and saved to database successfully!" });
        }

        [HttpPost("admin/accounts/deactivate")]
        public async Task<IActionResult> DeactivateAccount([FromBody] DeactivateRequestDto dto, [FromServices] ULDBContext context)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.UserId || u.Id.ToString() == dto.UserId);

            if (user == null)
            {
                return NotFound(new { message = $"User '{dto.UserId}' not found in database." });
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            return Ok(new { message = $"Account {dto.UserId} successfully deactivated and removed." });
        }

        // 2. Fleet Status & Telemetry
        [HttpGet("fleet/buses/available")]
        public IActionResult GetFleetStatus()
        {
            var fleet = new[]
            {
                new { Id = 1, FleetNumber = "BUS-4412", DriverName = "Jackson Cole", Status = "IN_SERVICE", FuelBatteryLevel = 84, IsFixed = true, Label = "Line 4 Crosstown" },
                new { Id = 2, FleetNumber = "TR-204", DriverName = "Elena Rostova", Status = "MAINTENANCE", FuelBatteryLevel = 32, IsFixed = false, Label = "Line 1 Loop" }
            };
            return Ok(fleet);
        }

        [HttpPut("fleet/{vehicleId}/status")]
        public IActionResult ToggleVehicleStatus(string vehicleId, [FromBody] ToggleStatusDto dto)
        {
            return Ok(new { message = $"Vehicle {vehicleId} status updated. IsFixed: {dto.IsFixed}" });
        }

        // 3. Operations & Active Trips
        [HttpPost("admin/fare-discount")]
        public IActionResult ToggleFareDiscount([FromBody] DiscountDto dto)
        {
            return Ok(new { message = $"Fare discount active set to {dto.Active}" });
        }

        [HttpGet("admin/trips/active")]
        public IActionResult SearchActiveTrips([FromQuery] string query)
        {
            var results = new[]
            {
                new { CommuterName = query ?? "Unknown", CardId = "SA-904812", Route = "BRT Line 7", CurrentStation = "Terminal 4 (In Transit)", Status = "ACTIVE" }
            };
            return Ok(results);
        }
        [HttpGet("admin/accounts")]
        public async Task<IActionResult> GetAllAccounts([FromServices] ULDBContext context)
        {
            var users = await context.Users
                .Select(u => new
                {
                    Id = u.Id.ToString(),
                    FullName = u.Name,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    Status = "ACTIVE" // Adjust if your User entity tracks status differently
                })
                .ToListAsync();

            return Ok(users);
        }
        // 4. Refunds & Reports
        [HttpPost("ticket/refund")]
        public IActionResult ProcessRefund([FromBody] RefundRequest dto)
        {
            return Ok(new { message = $"Refund of R{dto.Amount} processed for transaction {dto.TransactionId}" });
        }

        [HttpGet("conductorshift/reports")]
        public IActionResult GenerateShiftReport([FromQuery] string date)
        {
            byte[] pdfBytes;
            if (System.IO.File.Exists("sample.pdf"))
            {
                pdfBytes = System.IO.File.ReadAllBytes("sample.pdf");
            }
            else
            {
                pdfBytes = System.Text.Encoding.UTF8.GetBytes("%PDF-1.4 Mock PDF Content for Shift Report");
            }

            return File(pdfBytes, "application/pdf", $"UrbanLink_Shift_Report_{date}.pdf");
        }
    }

    // --- DTO Classes matching frontend request bodies ---
    public record CreateAccountRequest(string FullName, string Email, string Role);
    public record DeactivateRequestDto(string UserId, string Reason);
    public record ToggleStatusDto(bool IsFixed, int Status);
    public record DiscountDto(bool Active);
    public record RefundRequest(string TransactionId, decimal Amount);
}