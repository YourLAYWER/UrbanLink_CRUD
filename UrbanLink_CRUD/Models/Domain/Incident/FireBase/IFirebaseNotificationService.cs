namespace UrbanLink_CRUD.Models.Domain.Incident.FireBase
{
    public interface IFirebaseNotificationService
    {
        // Sends a push notification to anyone subscribed to "route_{routeId}_alerts"
        Task SendRouteAlertAsync(int routeId, string title, string message);
    }
}
