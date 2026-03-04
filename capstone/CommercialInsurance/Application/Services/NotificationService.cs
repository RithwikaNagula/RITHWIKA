using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Hubs;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IGenericRepository<Notification> _repository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            IGenericRepository<Notification> repository,
            IHubContext<NotificationHub> hubContext)
        {
            _repository = repository;
            _hubContext = hubContext;
        }

        public async Task CreateNotificationAsync(string userId, string title, string message, string type, string? relatedEntityId = null)
        {
            if (!Enum.TryParse<NotificationType>(type, true, out var notificationType))
            {
                notificationType = NotificationType.System;
            }

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = notificationType,
                RelatedEntityId = relatedEntityId
            };

            await _repository.AddAsync(notification);

            var dto = new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type.ToString(),
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                RelatedEntityId = notification.RelatedEntityId
            };

            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", dto);
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId)
        {
            var notifications = await _repository.FindAsync(n => n.UserId == userId);
            return notifications.OrderByDescending(n => n.CreatedAt).Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type.ToString(),
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                RelatedEntityId = n.RelatedEntityId
            });
        }

        public async Task MarkAsReadAsync(string notificationId)
        {
            var notification = await _repository.GetByIdAsync(notificationId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _repository.UpdateAsync(notification);
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = await _repository.FindAsync(n => n.UserId == userId && !n.IsRead);
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                await _repository.UpdateAsync(notification);
            }
        }
    }
}
