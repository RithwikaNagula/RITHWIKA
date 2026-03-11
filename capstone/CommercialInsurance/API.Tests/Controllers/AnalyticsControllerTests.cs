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

namespace API.Tests.Controllers
{
    public class AnalyticsControllerTests
    {
        private readonly Mock<IAnalyticsService> _serviceMock;
        private readonly AnalyticsController _controller;

        public AnalyticsControllerTests()
        {
            _serviceMock = new Mock<IAnalyticsService>();
            _controller = new AnalyticsController(_serviceMock.Object);
        }

        // Validates that calling the revenue endpoint with a specified period successfully unwraps the projected trends in a 200 OK output
        [Fact]
        public async Task GetRevenueAnalytics_ShouldReturnOk()
        {
            // Arrange
            var response = new RevenueAnalyticsDto();
            _serviceMock.Setup(s => s.GetRevenueAnalyticsAsync("monthly")).ReturnsAsync(response);

            // Act
            var result = await _controller.GetRevenueAnalytics("monthly");

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }
    }
}
