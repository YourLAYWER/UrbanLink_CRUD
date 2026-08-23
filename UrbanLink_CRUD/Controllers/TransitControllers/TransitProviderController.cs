using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;

namespace UrbanLink_CRUD.Controllers.TransitControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransitProviderController : ControllerBase
    {

        private readonly ULDBContext _context;

        public TransitProviderController(ULDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a lightweight list of transit providers for UI dropdowns.
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetProviderSummaries()
        {
            var providers = await _context.TransitProviders
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name
                })
                // Order them alphabetically
                .OrderBy(p => p.Name)
                .ToListAsync();

            if (providers.Count == 0)
            {
                return NotFound(new { message = "No transit providers found." });
            }

            return Ok(providers);
        }

    }
}
