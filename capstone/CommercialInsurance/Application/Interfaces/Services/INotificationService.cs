// Service contract for creating and delivering in-app notifications via SignalR.
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface INotificationService
    {
        // Persists a notification to the DB and pushes it to the user's SignalR channel in real time
        Task CreateNotificationAsync(string userId, string title, string message, string type, string? relatedEntityId = null);
        // Retrieves all notifications for a user, ordered by most recent first
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId);
        // Marks a single notification as read so it no longer appears in the unread badge count
        Task MarkAsReadAsync(string notificationId);
        // Bulk-marks all of the user's notifications as read
        Task MarkAllAsReadAsync(string userId);
    }
}
