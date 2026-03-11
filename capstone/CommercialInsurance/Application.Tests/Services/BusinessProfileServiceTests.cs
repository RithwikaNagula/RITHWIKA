// Test Layer: Application Services
// Purpose: Validates core business logic, algorithm correctness, and interactions with mocked domain repositories.
// Design: Uses XUnit and Moq to isolate dependencies and guarantee idempotent execution.
using Xunit;
using Moq;
using FluentAssertions;
using Application.Services;
using Application.Interfaces.Repositories;
using Domain.Entities;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Tests.Services
{
    public class BusinessProfileServiceTests
    {
        private readonly Mock<IBusinessProfileRepository> _profileRepo;
        private readonly BusinessProfileService _service;

        public BusinessProfileServiceTests()
        {
            _profileRepo = new Mock<IBusinessProfileRepository>();
            _service = new BusinessProfileService(_profileRepo.Object);
        }

        // Ensures that querying a business profile by its owner's ID accurately fetches the matching DB record
        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnProfile()
        {
            // Arrange
            var profile = new BusinessProfile { UserId = "u1", BusinessName = "TechCorp" };
            _profileRepo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(profile);

            // Act
            var result = await _service.GetByUserIdAsync("u1");

            // Assert
            result.Should().NotBeNull();
            result!.BusinessName.Should().Be("TechCorp");
        }

        // Asserts that upserting a payload for an unrecognized profile correctly invokes an insert operation via the repository
        [Fact]
        public async Task UpsertProfileAsync_ShouldInvokeRepo()
        {
            // Arrange
            var dto = new CreateBusinessProfileDto { BusinessName = "New" };
            _profileRepo.Setup(r => r.GetProfilesByUserIdAsync("u1")).ReturnsAsync(new List<BusinessProfile>());
            _profileRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<BusinessProfile>());

            // Act
            await _service.UpsertProfileAsync("u1", dto);

            // Assert
            _profileRepo.Verify(r => r.AddAsync(It.IsAny<BusinessProfile>()), Times.Once);
        }
    }
}
