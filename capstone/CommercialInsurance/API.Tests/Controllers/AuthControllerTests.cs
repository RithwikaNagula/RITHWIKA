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
using System;

namespace API.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthController(_authServiceMock.Object);
        }

        // Asserts that providing valid credentials safely routes the payload through the auth pipeline and yields a token in a 200 OK response
        [Fact]
        public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "test@example.com", Password = "password" };
            var response = new LoginResponseDto { Token = "token123" };
            _authServiceMock.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync(response);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            (result as OkObjectResult)!.Value.Should().Be(response);
        }

        // Confirms that passing a correctly formatted new user payload seamlessly creates an identity and returns a tracking 201 Created result
        [Fact]
        public async Task Register_ShouldReturnCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var registerDto = new RegisterDto { Email = "new@example.com" };
            var response = new UserDto { Id = "u1", Email = "new@example.com" };
            _authServiceMock.Setup(s => s.RegisterAsync(registerDto)).ReturnsAsync(response);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
        }
    }
}
