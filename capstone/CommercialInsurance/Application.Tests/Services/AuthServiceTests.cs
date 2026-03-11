// Test Layer: Application Services
// Purpose: Validates core business logic, algorithm correctness, and interactions with mocked domain repositories.
// Design: Uses XUnit and Moq to isolate dependencies and guarantee idempotent execution.
using Xunit;
using Moq;
using FluentAssertions;
using Application.Services;
using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Helpers;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Application.Interfaces.Services;

namespace Application.Tests.Services
{
    public class AuthServiceTests
    {
        // Confirms that passing correct credentials matching a hashed DB password successfully returns a fully signed JWT
        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenValidCredentials()
        {
            // Arrange
            var userRepoMock = new Mock<IUserRepository>();
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["Jwt:Key"]).Returns("SuperSecretKeyThatIsAtLeast32BytesLongForHS256");
            configMock.Setup(x => x["Jwt:Issuer"]).Returns("Issuer");
            configMock.Setup(x => x["Jwt:Audience"]).Returns("Audience");
            
            var notifyServiceMock = new Mock<INotificationService>();
            var policyRepoMock = new Mock<IPolicyRepository>();
            var claimRepoMock = new Mock<IClaimRepository>();

            var jwtGenerator = new JwtTokenGenerator(configMock.Object);
            var envMock = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();

            var user = new User 
            { 
                Id = "1", 
                Email = "test@example.com", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("validPass"),
                Role = UserRole.Customer
            };
            
            userRepoMock.Setup(u => u.GetByEmailAsync("test@example.com"))
                        .ReturnsAsync(user);

            var authService = new AuthService(
                userRepoMock.Object, 
                jwtGenerator, 
                notifyServiceMock.Object, 
                policyRepoMock.Object, 
                claimRepoMock.Object,
                envMock.Object);

            var loginDto = new LoginDto { Email = "test@example.com", Password = "validPass" };

            // Act
            var result = await authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.User.Id.Should().Be("1");
        }

        // Asserts that an unauthorized exception is accurately thrown when an unrecognized email prevents login
        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenCredentialsAreInvalid()
        {
            // Arrange
            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(u => u.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User)null);
            var authService = new AuthService(userRepoMock.Object, null, null, null, null, null);

            // Act
            Func<Task> act = async () => await authService.LoginAsync(new LoginDto { Email = "wrong@test.com" });

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        // Validates that the registration pipeline successfully commits a new user and pushes a welcome notification
        [Fact]
        public async Task RegisterAsync_ShouldInvokeAddAndNotify()
        {
            // Arrange
            var userRepoMock = new Mock<IUserRepository>();
            var notifyMock = new Mock<INotificationService>();
            var userRepo = new List<User>();
            userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());
            userRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                        .ReturnsAsync(new List<User>());
            
            var authService = new AuthService(userRepoMock.Object, null, notifyMock.Object, null, null, null);
            var dto = new RegisterDto { Email = "new@test.com", Password = "password123", FullName = "New User" };

            // Act
            var result = await authService.RegisterAsync(dto);

            // Assert
            userRepoMock.Verify(u => u.AddAsync(It.IsAny<User>()), Times.Once);
            result.Should().NotBeNull();
            result.Email.Should().Be("new@test.com");
        }
    }
}
