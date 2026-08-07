using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Entities.Transit;

namespace UrbanLink_CRUD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JourneyLegController : ControllerBase
    {
        private readonly ULDBContext _context;

        public JourneyLegController(ULDBContext context)
        {
            _context = context;
        }

        // GET: api/JourneyLeg
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JourneyLeg>>> GetJourneyLegs()
        {
            var journeyLegs = await _context.JourneyLegs
                .Include(j => j.Journey)
                .Include(j => j.Route)
                .Include(j => j.BoardingStop)
                .Include(j => j.DestinationStop)
                .ToListAsync();

            return Ok(journeyLegs);
        }
    }
}