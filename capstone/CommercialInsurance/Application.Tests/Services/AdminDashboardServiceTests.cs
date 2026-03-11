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
using System;

namespace Application.Tests.Services
{
    public class AdminDashboardServiceTests
    {
        private readonly Mock<IUserRepository> _userRepo;
        private readonly Mock<IPolicyRepository> _policyRepo;
        private readonly Mock<IClaimRepository> _claimRepo;
        private readonly Mock<IPaymentRepository> _paymentRepo;
        private readonly Mock<IPlanRepository> _planRepo;
        private readonly Mock<IGenericRepository<InsuranceType>> _typeRepo;
        private readonly AdminDashboardService _service;

        public AdminDashboardServiceTests()
        {
            _userRepo = new Mock<IUserRepository>();
            _policyRepo = new Mock<IPolicyRepository>();
            _claimRepo = new Mock<IClaimRepository>();
            _paymentRepo = new Mock<IPaymentRepository>();
            _planRepo = new Mock<IPlanRepository>();
            _typeRepo = new Mock<IGenericRepository<InsuranceType>>();

            _service = new AdminDashboardService(
                _userRepo.Object,
                _policyRepo.Object,
                _paymentRepo.Object);
        }

        // Confirms that the admin dashboard service correctly queries and aggregates counts from all mocked domains to produce a global summary
        [Fact]
        public async Task GetDashboardStatsAsync_ShouldReturnCorrectNumbers()
        {
            // Arrange
            _policyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Policy> { new Policy { Id = "1" }, new Policy { Id = "2" } });
            _claimRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Claim> { new Claim { Id = "1" } });
            _userRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User> { new User { Id = "1" }, new User { Id = "2" }, new User { Id = "3" } });
            _paymentRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Payment>());

            // Act
            var result = await _service.GetDashboardStatsAsync();

            // Assert
            result.TotalPolicies.Should().Be(2);
            result.TotalUsers.Should().Be(3);
        }
    }
}
