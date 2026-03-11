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

namespace API.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IAdminDashboardService> _serviceMock;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _serviceMock = new Mock<IAdminDashboardService>();
            _controller = new AdminController(_authServiceMock.Object, _serviceMock.Object);
        }

        // Verifies that the AdminController correctly routes to its service and returns a 200 OK response with the expected global stats
        [Fact]
        public async Task GetDashboardStats_ShouldReturnOk()
        {
            // Arrange
            var stats = new AdminDashboardDto { TotalPolicies = 10 };
            _serviceMock.Setup(s => s.GetDashboardStatsAsync()).ReturnsAsync(stats);

            // Act
            var result = await _controller.GetDashboardStats();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            (result as OkObjectResult)!.Value.Should().Be(stats);
        }
    }
}
