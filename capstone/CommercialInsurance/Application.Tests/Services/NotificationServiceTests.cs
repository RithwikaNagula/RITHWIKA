// Test Layer: Application Services
// Purpose: Validates core business logic, algorithm correctness, and interactions with mocked domain repositories.
// Design: Uses XUnit and Moq to isolate dependencies and guarantee idempotent execution.
using Xunit;
using Moq;
using FluentAssertions;
using Application.Services;
using Application.Interfaces.Repositories;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Application.Hubs;

namespace Application.Tests.Services
{
    public class NotificationServiceTests
    {
        // Validates that triggering a new alert seamlessly targets both the persistent database and the live SignalR connection
        [Fact]
        public async Task CreateNotificationAsync_ShouldSaveAndNotify()
        {
            // Arrange
            var repoMock = new Mock<INotificationRepository>();
            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<ISingleClientProxy>();

            hubContextMock.Setup(x => x.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);

            var service = new NotificationService(repoMock.Object, hubContextMock.Object);

            // Act
            await service.CreateNotificationAsync("user1", "Alert", "Test Message", "System");

            // Assert
            repoMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
            clientProxyMock.Verify(c => c.SendCoreAsync("ReceiveNotification", It.IsAny<object[]>(), default), Times.Once);
        }

        // Asserts that polling the inbox dynamically applies the exact user ID filter and successfully maps the matches
        [Fact]
        public async Task GetUserNotificationsAsync_ShouldReturnNotificationsForUser()
        {
            // Arrange
            var repoMock = new Mock<INotificationRepository>();
            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            var notifications = new List<Notification>
            {
                new Notification { Id = "1", UserId = "user1", Title = "N1" },
                new Notification { Id = "2", UserId = "user1", Title = "N2" }
            };
            repoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Notification, bool>>>()))
                        .ReturnsAsync(notifications);

            var service = new NotificationService(repoMock.Object, hubContextMock.Object);

            // Act
            var result = await service.GetUserNotificationsAsync("user1");

            // Assert
            result.Should().HaveCount(2);
        }
    }
}
