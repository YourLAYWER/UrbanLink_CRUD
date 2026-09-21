using FirebaseAdmin.Messaging;

namespace UrbanLink_CRUD.Models.Domain.Incident.FireBase
{
    public class FirebaseNotificationService : IFirebaseNotificationService
    {
        public async Task SendRouteAlertAsync(int routeId, string title, string message)
        {
            try
            {
                // Format must exactly match what the Android app subscribes to
                string topic = $"route_{routeId}_alerts";

                var fcmMessage = new Message()
                {
                    Topic = topic,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = message
                    },
                    // The Android app can read this silent data payload to trigger UI updates
                    // or fetch the fresh status from the SQL database
                    Data = new Dictionary<string, string>()
                    {
                        { "routeId", routeId.ToString() },
                        { "action", "REFRESH_ROUTE_STATUS" }
                    }
                };

                // FirebaseMessaging.DefaultInstance is initialized once at startup
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(fcmMessage);

                Console.WriteLine($"Successfully sent FCM message: {response}");
            }
            catch (Exception ex)
            {
                // Log this failure, but don't crash the incident logging process!
                // If FCM fails, the SQL table still acts as the source of truth.
                Console.WriteLine($"Error sending FCM message: {ex.Message}");
            }
        }
    }
}
