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

namespace API.Tests.Controllers
{
    public class AgentDashboardControllerTests
    {
        private readonly Mock<IAgentDashboardService> _serviceMock;
        private readonly AgentDashboardController _controller;

        public AgentDashboardControllerTests()
        {
            _serviceMock = new Mock<IAgentDashboardService>();
            _controller = new AgentDashboardController(_serviceMock.Object);

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "agent1") };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        // Ensures the AgentDashboardController accurately extracts the logged-in agent's claims ID and fetches targeted insights, returning a 200 OK result
        [Fact]
        public async Task GetDashboardStats_ShouldReturnOk()
        {
            // Arrange
            var stats = new AgentDashboardDto { AssignedPolicies = 5 };
            _serviceMock.Setup(s => s.GetAgentDashboardStatsAsync("agent1")).ReturnsAsync(stats);

            // Act
            var result = await _controller.GetDashboardStats();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            (result as OkObjectResult)!.Value.Should().Be(stats);
        }
    }
}
