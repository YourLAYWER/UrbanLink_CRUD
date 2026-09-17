using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace UrbanLink_CRUD.Models.Domain
{
    public class TransitHub : Hub
    {
        // When a tablet boots up, it joins the group for its specific Shift or Bus
        public async Task JoinBusGroup(string BusId)
        {
            string groupName = $"bus_{BusId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // --- ADD THIS LINE TO CHECK SERVER CONSOLE ---
            Console.WriteLine($"[SignalR] Connection {Context.ConnectionId} successfully joined group: {groupName}");
        }

        // Leave group if shifting/logging out
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"[SignalR] Connection disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
