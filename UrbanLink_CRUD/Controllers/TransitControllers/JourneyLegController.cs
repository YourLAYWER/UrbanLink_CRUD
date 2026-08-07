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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JourneyLeg>>> GetJourneyLegs()
        {
            return await _context.JourneyLegs
                .Include(j => j.Journey)
                .Include(j => j.Route)
                .Include(j => j.BoardingStop)
                .Include(j => j.DestinationStop)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JourneyLeg>> GetJourneyLeg(int id)
        {
            var journeyLeg = await _context.JourneyLegs
                .Include(j => j.Journey)
                .Include(j => j.Route)
                .Include(j => j.BoardingStop)
                .Include(j => j.DestinationStop)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (journeyLeg == null)
                return NotFound();

            return journeyLeg;
        }

        //[HttpPost]
        //public async Task<IActionResult> CreateJourneyLeg(CreateJourneyLegDto dto)
        //{
        //    var journeyExists = await _context.Journeys
        //    .AnyAsync(j => j.Id == dto.JourneyId);

        //    if (!journeyExists)
        //    {
        //        return BadRequest(
        //            $"Journey with ID {dto.JourneyId} does not exist."
        //        );
        //    }


        //    var journeyLeg = new JourneyLeg
        //    {
        //        JourneyId = dto.JourneyId,
        //        SequenceOrder = dto.SequenceOrder,
        //        RouteId = dto.RouteId,
        //        BoardingStopId = dto.BoardingStopId,
        //        DestinationStopId = dto.DestinationStopId,
        //        LegFare = dto.LegFare,
        //        LegDurationMinutes = dto.LegDurationMinutes
        //    };

        //    _context.JourneyLegs.Add(journeyLeg);

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("ERROR:");
        //        Console.WriteLine(ex.Message);

        //        if (ex.InnerException != null)
        //        {
        //            Console.WriteLine("INNER ERROR:");
        //            Console.WriteLine(ex.InnerException.Message);
        //        }

        //        throw;
        //    }

        //    return CreatedAtAction(
        //        nameof(GetJourneyLeg),
        //        new { id = journeyLeg.Id },
        //        journeyLeg
        //    );
        //}

        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateJourneyLeg(int id, JourneyLeg journeyLeg)
        //{
        //    if (id != journeyLeg.Id)
        //        return BadRequest();

        //    _context.Entry(journeyLeg).State = EntityState.Modified;

        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteJourneyLeg(int id)
        //{
        //    var journeyLeg = await _context.JourneyLegs.FindAsync(id);

        //    if (journeyLeg == null)
        //        return NotFound();

        //    _context.JourneyLegs.Remove(journeyLeg);

        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}
    }
}