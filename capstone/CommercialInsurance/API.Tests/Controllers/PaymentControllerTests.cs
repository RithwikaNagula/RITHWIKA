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
    public class PaymentControllerTests
    {
        private readonly Mock<IPaymentService> _serviceMock;
        private readonly PaymentController _controller;

        public PaymentControllerTests()
        {
            _serviceMock = new Mock<IPaymentService>();
            _controller = new PaymentController(_serviceMock.Object);

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user1") };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        // Validates that requesting payments for a specific policy correctly forwards the lookup and returns a 200 OK wrapper
        [Fact]
        public async Task GetByPolicy_ShouldReturnOk()
        {
            // Arrange
            var payments = new List<PaymentDto> { new PaymentDto { Id = "pay1" } };
            _serviceMock.Setup(s => s.GetPaymentsByPolicyAsync("pol1")).ReturnsAsync(payments);

            // Act
            var result = await _controller.GetByPolicy("pol1");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
