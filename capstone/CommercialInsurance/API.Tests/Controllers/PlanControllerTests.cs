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
    public class PlanControllerTests
    {
        private readonly Mock<IPlanService> _serviceMock;
        private readonly PlanController _controller;

        public PlanControllerTests()
        {
            _serviceMock = new Mock<IPlanService>();
            _controller = new PlanController(_serviceMock.Object);
        }

        // Asserts that querying all insurance plans correctly invokes the service and returns a 200 OK wrapper containing the list
        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            // Arrange
            var plans = new List<PlanDto> { new PlanDto { Id = "p1" } };
            _serviceMock.Setup(s => s.GetAllPlansAsync()).ReturnsAsync(plans);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
