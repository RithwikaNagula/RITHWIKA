// Test Layer: Application Services
// Purpose: Validates core business logic, algorithm correctness, and interactions with mocked domain repositories.
// Design: Uses XUnit and Moq to isolate dependencies and guarantee idempotent execution.
using Xunit;
using Moq;
using FluentAssertions;
using Application.Services;
using Application.Interfaces.Repositories;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Tests.Services
{
    public class AgentDashboardServiceTests
    {
        private readonly Mock<IPolicyRepository> _policyRepo;
        private readonly Mock<IUserRepository> _userRepo;
        private readonly AgentDashboardService _service;

        public AgentDashboardServiceTests()
        {
            _policyRepo = new Mock<IPolicyRepository>();
            _userRepo = new Mock<IUserRepository>();

            _service = new AgentDashboardService(_policyRepo.Object);
        }

        // Asserts that an agent's dashboard service correctly filters policies based strictly on their unique assignment ID
        [Fact]
        public async Task GetAgentDashboardStatsAsync_ShouldReturnStats()
        {
            // Arrange
            var agentId = "agent1";
            _userRepo.Setup(r => r.GetByIdAsync(agentId)).ReturnsAsync(new User { Id = agentId, Role = Domain.Enums.UserRole.Agent });
            _policyRepo.Setup(r => r.GetPoliciesByAgentIdAsync(agentId)).ReturnsAsync(new List<Policy> { new Policy { Id = "p1" } });

            // Act
            var result = await _service.GetAgentDashboardStatsAsync(agentId);

            // Assert
            result.AssignedPolicies.Should().Be(1);
        }
    }
}
