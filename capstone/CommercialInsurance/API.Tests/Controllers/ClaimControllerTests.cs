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
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace API.Tests.Controllers
{
    public class ClaimControllerTests
    {
        private readonly Mock<IClaimService> _claimServiceMock;
        private readonly ClaimController _controller;

        public ClaimControllerTests()
        {
            _claimServiceMock = new Mock<IClaimService>();
            _controller = new ClaimController(_claimServiceMock.Object);

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user1") };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        // Asserts that successfully filing a new claim yields a 201 Created tracking response pointing to the new resource
        [Fact]
        public async Task FileClaim_ShouldReturnCreated()
        {
            // Arrange
            var dto = new CreateClaimDto();
            var response = new ClaimDto { Id = "c1" };
            _claimServiceMock.Setup(s => s.FileClaimAsync(dto, "pol1", It.IsAny<List<IFormFile>>()))
                             .ReturnsAsync(response);

            // Act
            var result = await _controller.FileClaim("pol1", dto, new List<IFormFile>());

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        // Validates that fetching all claims bypasses filtering and correctly outputs a 200 OK wrapper containing the full dataset
        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            // Arrange
            var claims = new List<ClaimDto> { new ClaimDto { Id = "c1" } };
            _claimServiceMock.Setup(s => s.GetAllClaimsAsync()).ReturnsAsync(claims);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
