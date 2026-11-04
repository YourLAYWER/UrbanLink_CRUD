using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Timers;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Domain;
using Microsoft.AspNetCore.SignalR;
using UrbanLink_CRUD.Models.Domain.Journey;
using UrbanLink_CRUD.Models.Dtos.TransitDtos;
using UrbanLink_CRUD.Models.Entities.Transit;
using UrbanLink_CRUD.Models.Enums;
using UrbanLink_CRUD.Services.RoutingLogic;
using UrbanLink_CRUD.Models.Domain.Payment;
using RoutingContext = UrbanLink_CRUD.Services.RoutingLogic.RoutingContext;


namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimulatorController : ControllerBase
    {
        private readonly IHubContext<TransitHub> _hub;

        public SimulatorController(IHubContext<TransitHub> hub)
        {
            _hub = hub;
        }

        [HttpPost("StartRoute1")]
        public IActionResult SimulateRoute1()
        {
            // Fire and forget a background thread to loop coordinates
            Task.Run(async () =>
            {
                // Starting coordinates in Gqeberha (e.g., NMU Summerstrand)
                double currentLat = -33.956009;
                double currentLng = 25.588995;

                for (int i = 0; i < 20; i++)
                {
                    currentLat += 0.0005; // Simulate movement North
                    currentLng -= 0.0002; // Simulate movement West

                    var payload = new BusLocationUpdateDto
                    {
                        BusId = 101,
                        RouteId = 1,
                        Latitude = currentLat,
                        Longitude = currentLng
                    };

                    await _hub.Clients.Group("Route_1").SendAsync("ReceiveBusLocation", payload);
                    await Task.Delay(3000); // Wait 3 seconds like the real driver app
                }
            });

            return Ok("Route 1 Simulation Started for 60 seconds.");
        }
    }
}