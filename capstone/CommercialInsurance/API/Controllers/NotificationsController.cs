// Returns unread notifications for the authenticated user and provides a mark-as-read endpoint.
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Notifications are always user-scoped; unauthenticated requests are rejected
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        // INotificationService handles both database persistence and SignalR push delivery
        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET api/notifications
        // Returns the authenticated user's unread notification list.
        // The frontend polls or subscribes to this to show the notification bell badge count.
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            // User ID is extracted from the JWT claim; prevents users from reading each other's notifications
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(result);
        }

        // PUT api/notifications/{id}/read
        // Marks a single notification as read and decrements the unread badge count on the client.
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return NoContent(); // 204 — success with no response body needed
        }

        // PUT api/notifications/mark-all-read
        // Marks every unread notification for the current user as read in a single call.
        // More efficient than calling PUT /{id}/read for each notification individually.
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _notificationService.MarkAllAsReadAsync(userId);
            return NoContent();
        }
    }
}
