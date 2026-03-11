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
using System.Linq;

namespace API.Tests.Controllers
{
    public class InsuranceTypeControllerTests
    {
        private readonly Mock<IInsuranceTypeService> _serviceMock;
        private readonly InsuranceTypeController _controller;

        public InsuranceTypeControllerTests()
        {
            _serviceMock = new Mock<IInsuranceTypeService>();
            _controller = new InsuranceTypeController(_serviceMock.Object);
        }

        // Confirms that requesting all insurance categories directly queries the service and returns a 200 OK wrapper
        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            // Arrange
            var types = new List<InsuranceTypeDto> { new InsuranceTypeDto { Id = "t1" } };
            _serviceMock.Setup(s => s.GetAllInsuranceTypesAsync()).ReturnsAsync(types);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
