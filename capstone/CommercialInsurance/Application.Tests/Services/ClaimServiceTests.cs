// Test Layer: Application Services
// Purpose: Validates core business logic, algorithm correctness, and interactions with mocked domain repositories.
// Design: Uses XUnit and Moq to isolate dependencies and guarantee idempotent execution.
using Xunit;
using Moq;
using FluentAssertions;
using Application.Services;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Application.DTOs;

namespace Application.Tests.Services
{
    public class ClaimServiceTests
    {
        private readonly Mock<IClaimRepository> _claimRepoMock;
        private readonly Mock<IPolicyRepository> _policyRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IGenericRepository<ClaimHistoryLog>> _logRepoMock;
        private readonly Mock<IGenericRepository<Document>> _docRepoMock;
        private readonly Mock<IBusinessProfileRepository> _profileRepoMock;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly ClaimService _service;

        public ClaimServiceTests()
        {
            _claimRepoMock = new Mock<IClaimRepository>();
            _policyRepoMock = new Mock<IPolicyRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _logRepoMock = new Mock<IGenericRepository<ClaimHistoryLog>>();
            _docRepoMock = new Mock<IGenericRepository<Document>>();
            _profileRepoMock = new Mock<IBusinessProfileRepository>();
            _envMock = new Mock<IWebHostEnvironment>();
            _notificationServiceMock = new Mock<INotificationService>();

            _service = new ClaimService(
                _claimRepoMock.Object,
                _policyRepoMock.Object,
                _userRepoMock.Object,
                _logRepoMock.Object,
                _docRepoMock.Object,
                _profileRepoMock.Object,
                _envMock.Object,
                _notificationServiceMock.Object);
        }

        // Validates that filing a claim against an active policy successfully commits the generic claim record to the database
        [Fact]
        public async Task FileClaimAsync_ShouldCreateClaim_WhenPolicyIsActive()
        {
            // Arrange
            var policy = new Policy { Id = "pol1", Status = PolicyStatus.Active, UserId = "user1", SelectedCoverageAmount = 5000 };
            _policyRepoMock.Setup(r => r.GetByIdAsync("pol1")).ReturnsAsync(policy);
            _userRepoMock.Setup(r => r.GetByIdAsync("user1")).ReturnsAsync(new User { Id = "user1", AssignedClaimsOfficerId = "off1" });
            _userRepoMock.Setup(r => r.GetByIdAsync("off1")).ReturnsAsync(new User { Id = "off1", Role = UserRole.ClaimsOfficer });
            
            // Return dummy claim when GetById is called after creation
            _claimRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                         .ReturnsAsync(new Claim { Id = "clm1", PolicyId = "pol1" });

            var dto = new CreateClaimDto { ClaimAmount = 1000, Description = "Accident" };
            
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.Length).Returns(1024);
            var files = new List<IFormFile> { fileMock.Object };

            // Act
            var result = await _service.FileClaimAsync(dto, "pol1", files);

            // Assert
            _claimRepoMock.Verify(r => r.AddAsync(It.IsAny<Claim>()), Times.Once);
            result.Should().NotBeNull();
        }

        // Confirms that fetching all claims bypasses filters and correctly retrieves all raw entity records into DTO wrappers
        [Fact]
        public async Task GetAllClaimsAsync_ShouldReturnClaims()
        {
            // Arrange
            var claims = new List<Claim> { new Claim { Id = "c1", PolicyId = "pol1" } };
            _claimRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(claims);
            _policyRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new Policy());

            // Act
            var result = await _service.GetAllClaimsAsync();

            // Assert
            result.Should().HaveCount(1);
        }
    }
}
