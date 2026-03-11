// Manages system alerts, persists them to the database, and uses SignalR to broadcast real-time updates to connected clients.
using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        // Injects standard DB repo alongside the SignalR Hub Context. 
        // HubContext enables server-to-client push commands outside of a direct HTTP request cycle.
        private readonly IHubContext<Application.Hubs.NotificationHub> _hubContext;

        public NotificationService(
            INotificationRepository notificationRepository,
            IHubContext<Application.Hubs.NotificationHub> hubContext)
        {
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
        }

        // Retrieves all unread alerts for a given user, sorted newest-first.
        // Used primarily to populate the drop-down bell menu on page load.
        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId)
        {
            var notifications = await _notificationRepository.FindAsync(n => n.UserId == userId && !n.IsRead);
            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type.ToString(),
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead
            }).OrderByDescending(n => n.CreatedAt);
        }

        // Creates a DB record AND attempts immediate WebSocket delivery.
        public async Task CreateNotificationAsync(string userId, string title, string message, string type, string? referenceId = null)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = Enum.TryParse<Domain.Enums.NotificationType>(type, true, out var parsedType) ? parsedType : Domain.Enums.NotificationType.System,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _notificationRepository.AddAsync(notification);

            // Using SignalR groups: Each authenticated user joins a group named after their ID.
            // Fire-and-forget message "ReceiveNotification" pushes the alert dynamically 
            // without requiring a page refresh on the client side.
            await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", new NotificationDto
            {
                Id = notification.Id,
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = notification.CreatedAt,
                IsRead = false
            });


        }

        // Toggles a single notification's state; removes it from the unread queue.
        public async Task MarkAsReadAsync(string id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(notification);
            }
        }

        // Clears the unread queue entirely; invoked when a user clicks "Mark all as read" in the UI.
        public async Task MarkAllAsReadAsync(string userId)
        {
            var unread = await _notificationRepository.FindAsync(n => n.UserId == userId && !n.IsRead);
            foreach (var r in unread)
            {
                r.IsRead = true;
                await _notificationRepository.UpdateAsync(r);
            }
        }
    }
}
