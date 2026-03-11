// SignalR hub that pushes real-time notifications to connected clients; clients join a user-specific group on connect.
using Microsoft.AspNetCore.SignalR;
using Application.DTOs;

namespace Application.Hubs
{
    /// <summary>
    /// SignalR hub for real-time notification delivery.
    /// Used by NotificationService via IHubContext to push notifications to connected clients.
    /// Connection management and user mapping are handled automatically by SignalR using JWT claims.
    /// </summary>
    public class NotificationHub : Hub
    {
    }
}
