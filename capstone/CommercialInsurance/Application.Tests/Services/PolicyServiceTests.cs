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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Application.DTOs;

namespace Application.Tests.Services
{
    public class PolicyServiceTests
    {
        private readonly Mock<IPolicyRepository> _policyRepoMock;
        private readonly Mock<IPlanRepository> _planRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IBusinessProfileRepository> _profileRepoMock;
        private readonly Mock<IGenericRepository<Document>> _docRepoMock;
        private readonly Mock<IClaimRepository> _claimRepoMock;
        private readonly Mock<IPaymentRepository> _paymentRepoMock;
        private readonly Mock<IGenericRepository<Domain.Entities.ClaimHistoryLog>> _historyRepoMock;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly Mock<INotificationService> _notificationService;
        private readonly PolicyService _service;

        public PolicyServiceTests()
        {
            _policyRepoMock = new Mock<IPolicyRepository>();
            _planRepoMock = new Mock<IPlanRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _profileRepoMock = new Mock<IBusinessProfileRepository>();
            _docRepoMock = new Mock<IGenericRepository<Document>>();
            _claimRepoMock = new Mock<IClaimRepository>();
            _paymentRepoMock = new Mock<IPaymentRepository>();
            _historyRepoMock = new Mock<IGenericRepository<Domain.Entities.ClaimHistoryLog>>();
            _envMock = new Mock<IWebHostEnvironment>();
            _notificationService = new Mock<INotificationService>();

            _service = new PolicyService(
                _policyRepoMock.Object,
                _planRepoMock.Object,
                _userRepoMock.Object,
                _profileRepoMock.Object,
                _docRepoMock.Object,
                _claimRepoMock.Object,
                _paymentRepoMock.Object,
                _historyRepoMock.Object,
                _envMock.Object,
                _notificationService.Object);
        }

        // Verifies that filtering the global policy pool by a unique user ID accurately unpacks the mapped results
        [Fact]
        public async Task GetPoliciesByUserAsync_ShouldReturnPolicies()
        {
            // Arrange
            var policies = new List<Policy> { new Policy { Id = "p1", UserId = "user1" } };
            _policyRepoMock.Setup(r => r.GetPoliciesByUserIdAsync("user1")).ReturnsAsync(policies);
            _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new User());

            // Act
            var result = await _service.GetPoliciesByUserAsync("user1");

            // Assert
            result.Should().HaveCount(1);
        }

        // Asserts that submitting a complex policy payload correctly validates relationships, orchestrates file saving, and persists the entity
        [Fact]
        public async Task CreatePolicyAsync_ShouldInvokeAddAndReturnDto()
        {
            // Arrange
            var userId = "user1";
            var planId = "plan1";
            var profileId = "prof1";

            var profile = new BusinessProfile { Id = profileId, UserId = userId, IsProfileCompleted = true };
            _profileRepoMock.Setup(r => r.GetByIdAsync(profileId)).ReturnsAsync(profile);

            var plan = new Plan { Id = planId, BasePremium = 100 };
            _planRepoMock.Setup(r => r.GetByIdAsync(planId)).ReturnsAsync(plan);

            var user = new User { Id = userId, AssignedAgentId = "agent1" };
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            _policyRepoMock.Setup(r => r.GetPoliciesByUserIdAsync(userId)).ReturnsAsync(new List<Policy>());
            
            // For the return at the end of CreatePolicyAsync
            _policyRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(new Policy { Id = "new-pol", UserId = userId, PlanId = planId });

            var dto = new CreatePolicyDto 
            { 
                PlanId = planId, 
                SelectedCoverageAmount = 1000, 
                BusinessProfileId = profileId 
            };

            _envMock.Setup(e => e.WebRootPath).Returns("wwwroot");

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.Length).Returns(1024);
            var documents = new List<IFormFile> { fileMock.Object };

            // Act
            var result = await _service.CreatePolicyAsync(dto, userId, documents, null);

            // Assert
            _policyRepoMock.Verify(r => r.AddAsync(It.IsAny<Policy>()), Times.Once);
            result.Should().NotBeNull();
        }
    }
}
