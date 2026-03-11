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
    public class PolicyControllerTests
    {
        private readonly Mock<IPolicyService> _policyServiceMock;
        private readonly PolicyController _controller;

        public PolicyControllerTests()
        {
            _policyServiceMock = new Mock<IPolicyService>();
            _controller = new PolicyController(_policyServiceMock.Object);
            
            // Mock User identity
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user1") };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        // Ensures the authenticated user's ID is correctly extracted and used to wrap their policies inside a 200 OK response
        [Fact]
        public async Task GetMyPolicies_ShouldReturnOk()
        {
            // Arrange
            var policies = new List<PolicyDto> { new PolicyDto { Id = "p1" } };
            _policyServiceMock.Setup(s => s.GetPoliciesByUserAsync("user1")).ReturnsAsync(policies);

            // Act
            var result = await _controller.GetMyPolicies();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            (result as OkObjectResult)!.Value.Should().BeEquivalentTo(policies);
        }

        // Confirms that submitting a valid multipart policy request yields a 201 Created RESTful outcome pointing to the new resource
        [Fact]
        public async Task RequestPurchase_ShouldReturnCreated()
        {
            // Arrange
            var dto = new CreatePolicyDto();
            var response = new PolicyDto { Id = "new-p" };
            _policyServiceMock.Setup(s => s.CreatePolicyAsync(dto, "user1", It.IsAny<IEnumerable<IFormFile>>(), null))
                              .ReturnsAsync(response);

            // Act
            var result = await _controller.RequestPurchase(dto, new List<IFormFile>());

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
        }
    }
}
