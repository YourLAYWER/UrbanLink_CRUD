using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace UrbanLink_CRUD.Models.Domain
{
    public class TransitHub : Hub
    {
        // When a tablet boots up, it joins the group for its specific Shift or Bus
        public async Task JoinShiftGroup(string shiftId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, shiftId);
        }

        // Leave group if shifting/logging out
        public async Task LeaveShiftGroup(string shiftId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, shiftId);
        }
    }
}
