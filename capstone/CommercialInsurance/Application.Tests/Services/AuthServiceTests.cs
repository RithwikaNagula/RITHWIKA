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
                claimRepoMock.Object);

            var loginDto = new LoginDto { Email = "test@example.com", Password = "validPass" };

            // Act
            var result = await authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.User.Id.Should().Be("1");
        }
    }
}
