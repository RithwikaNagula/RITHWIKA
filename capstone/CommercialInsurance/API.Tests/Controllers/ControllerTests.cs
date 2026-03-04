using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Application.Interfaces.Services;
using Application.DTOs;
using API.Controllers;
using System.Security.Claims;

namespace API.Tests.Controllers
{
    public class ControllerTests
    {
        [Fact]
        public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
                           .ReturnsAsync(new LoginResponseDto { Token = "valid-token" });

            var controller = new AuthController(authServiceMock.Object);
            var loginDto = new LoginDto { Email = "test@example.com", Password = "validPassword" };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(new LoginResponseDto { Token = "valid-token" });
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsInvalid()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
                           .ThrowsAsync(new UnauthorizedAccessException("Invalid password"));

            var controller = new AuthController(authServiceMock.Object);
            var loginDto = new LoginDto { Email = "test@example.com", Password = "wrongPassword" };

            // Act
            Func<Task> act = async () => await controller.Login(loginDto);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task CreatePolicy_ShouldReturnCreatedResult_WhenValid()
        {
            // Arrange
            var policyServiceMock = new Mock<IPolicyService>();
            policyServiceMock.Setup(s => s.CreatePolicyAsync(It.IsAny<CreatePolicyDto>(), It.IsAny<string>(), It.IsAny<IEnumerable<IFormFile>>(), null))
                             .ReturnsAsync(new PolicyDto { Id = "policy-123" });
            
            var controller = new PolicyController(policyServiceMock.Object);
            
            // Setup User Claims for controller.User
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var createDto = new CreatePolicyDto();

            // Act
            var result = await controller.RequestPurchase(createDto, new List<IFormFile>());

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be("GetById");
            createdResult.RouteValues!["id"].Should().Be("policy-123");
        }

        [Fact]
        public async Task RenewPolicy_ShouldReturnBadRequest_WhenNotEligible()
        {
            // Arrange
            var policyServiceMock = new Mock<IPolicyService>();
            policyServiceMock.Setup(s => s.RenewPolicyAsync("invalid-policy"))
                             .ThrowsAsync(new InvalidOperationException("Not eligible for renewal"));
            
            var controller = new PolicyController(policyServiceMock.Object);

            // Act
            var result = await controller.RenewPolicy("invalid-policy");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
        }

        [Fact]
        public async Task GetPolicyById_ShouldReturnOk_WhenPolicyExists()
        {
            // Arrange
            var policyServiceMock = new Mock<IPolicyService>();
            var expectedPolicy = new PolicyDto { Id = "policy-123", PolicyNumber = "POL-123" };
            
            policyServiceMock.Setup(s => s.GetPolicyByIdAsync("policy-123"))
                             .ReturnsAsync(expectedPolicy);
            
            var controller = new PolicyController(policyServiceMock.Object);

            // Act
            var result = await controller.GetById("policy-123");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedPolicy);
        }
    }
}
