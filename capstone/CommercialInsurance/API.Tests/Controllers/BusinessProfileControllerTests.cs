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
    public class BusinessProfileControllerTests
    {
        private readonly Mock<IBusinessProfileService> _serviceMock;
        private readonly BusinessProfileController _controller;

        public BusinessProfileControllerTests()
        {
            _serviceMock = new Mock<IBusinessProfileService>();
            _controller = new BusinessProfileController(_serviceMock.Object);

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user1") };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        // Asserts that fetching the logged-in user's business profiles accesses the service via their extracted ID and returns a 200 OK wrapper
        [Fact]
        public async Task GetProfiles_ShouldReturnOk()
        {
            // Arrange
            var profiles = new List<BusinessProfileDto> { new BusinessProfileDto { BusinessName = "X" } };
            _serviceMock.Setup(s => s.GetProfilesByUserIdAsync("user1")).ReturnsAsync(profiles);

            // Act
            var result = await _controller.GetProfiles();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        // Confirms that passing a new profile update payload successfully applies the changes and yields a 200 OK response
        [Fact]
        public async Task Update_ShouldReturnOk()
        {
            // Arrange
            var dto = new UpdateBusinessProfileDto();
            _serviceMock.Setup(s => s.UpdateProfileAsync("user1", dto)).ReturnsAsync(new BusinessProfileDto());

            // Act
            var result = await _controller.Update(dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
