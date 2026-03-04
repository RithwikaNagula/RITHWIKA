using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string title, string message, string type, string? relatedEntityId = null);
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId);
        Task MarkAsReadAsync(string notificationId);
        Task MarkAllAsReadAsync(string userId);
    }
}
