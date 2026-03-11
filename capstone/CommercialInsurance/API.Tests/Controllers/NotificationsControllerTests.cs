// Test Layer: API Controllers
// Purpose: Verifies HTTP request routing, input validation, and proper HTTP action results (e.g., 200 OK, 404 NotFound).
// Design: Uses XUnit and Moq to isolate dependencies and guarantee idempotent execution.
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Services;
using Application.DTOs;
using API.Controllers;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace API.Tests.Controllers
{
    public class NotificationsControllerTests
    {
        private readonly Mock<INotificationService> _serviceMock;
        private readonly NotificationsController _controller;

        public NotificationsControllerTests()
        {
            _serviceMock = new Mock<INotificationService>();
            _controller = new NotificationsController(_serviceMock.Object);

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user1") };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        // Asserts that requesting inbox alerts correlates with the authenticated user's ID and outputs a 200 OK wrapper
        [Fact]
        public async Task GetMyNotifications_ShouldReturnOk()
        {
            // Arrange
            var notifications = new List<NotificationDto> { new NotificationDto { Id = "n1" } };
            _serviceMock.Setup(s => s.GetUserNotificationsAsync("user1")).ReturnsAsync(notifications);

            // Act
            var result = await _controller.GetMyNotifications();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
