using Microsoft.AspNetCore.SignalR;
using UrbanLink_CRUD.Models.Dtos.TransitDtos;
using UrbanLink_CRUD.Models.Domain.SignalR;

namespace UrbanLink_CRUD.Models.Domain
{
    public class TransitHub : Hub
    {
        private readonly TelemetryQueue _queue;


        // Inject the singleton queue
        public TransitHub(TelemetryQueue queue)
        {
            _queue = queue;
        }


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

        // --- 2. SUBSCRIPTIONS (Called by Commuters & Admins) ---

        // Commuter Android App calls this when viewing a specific route
        public async Task JoinRouteGroup(int routeId)
        {
            string groupName = $"Route_{routeId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Commuter leaves the group when they exit the map screen
        public async Task LeaveRouteGroup(int routeId)
        {
            string groupName = $"Route_{routeId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        // React Admin Dashboard calls this on load
        public async Task JoinAdminDashboard()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "AdminDashboard");
        }

        // --- 2. PUBLISHING (Called by Driver Android App) ---

        // Driver tablet calls this every 3 seconds
        // public async Task BroadcastBusLocation(BusLocationUpdateDto locationData)
        // {
        //     // 1. Instantly fan-out to all commuters waiting on this specific route
        //     string routeGroupName = $"Route_{locationData.RouteId}";
        //     await Clients.Group(routeGroupName).SendAsync("ReceiveBusLocation", locationData);

        //     // 2. Instantly fan-out to the global admin dashboard
        //     await Clients.Group("AdminDashboard").SendAsync("ReceiveBusLocation", locationData);

        //     // Note: Later, we will inject a Channel<T> here to queue this data for the SQL batcher!
        // }

        public async Task BroadcastBusLocation(BusLocationUpdateDto locationData)
        {
            // 1. Save to memory for the 5-minute batcher
            _queue.Enqueue(locationData);

            // 2. Broadcast live to commuters
            await Clients.Group($"Route_{locationData.RouteId}").SendAsync("ReceiveBusLocation", locationData);
        }
    }
}
