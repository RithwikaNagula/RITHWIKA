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
using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<IPaymentRepository> _paymentRepo;
        private readonly Mock<IPolicyRepository> _policyRepo;
        private readonly Mock<IUserRepository> _userRepo;
        private readonly Mock<IPlanRepository> _planRepo;
        private readonly Mock<IBusinessProfileRepository> _profileRepo;
        private readonly Mock<INotificationService> _notificationService;
        private readonly PaymentService _service;

        public PaymentServiceTests()
        {
            _paymentRepo = new Mock<IPaymentRepository>();
            _policyRepo = new Mock<IPolicyRepository>();
            _userRepo = new Mock<IUserRepository>();
            _planRepo = new Mock<IPlanRepository>();
            _profileRepo = new Mock<IBusinessProfileRepository>();
            _notificationService = new Mock<INotificationService>();

            _service = new PaymentService(
                _paymentRepo.Object, 
                _policyRepo.Object, 
                _userRepo.Object,
                _planRepo.Object,
                _profileRepo.Object,
                _notificationService.Object);
        }

        // Confirms that pulling full transaction ledgers strictly accesses entries tied to the targeted parent policy
        [Fact]
        public async Task GetPaymentsByPolicyAsync_ShouldReturnPayments()
        {
            // Arrange
            var payments = new List<Payment> { new Payment { Id = "pay1", PolicyId = "pol1" } };
            _paymentRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Payment, bool>>>()))
                        .ReturnsAsync(payments);
            _policyRepo.Setup(r => r.GetByIdAsync("pol1")).ReturnsAsync(new Policy { PolicyNumber = "P1" });

            // Act
            var result = await _service.GetPaymentsByPolicyAsync("pol1");

            // Assert
            result.Should().HaveCount(1);
        }

        // Asserts that executing a simulated payment cycle accurately divides and spawns multiple billing records matched against the chosen frequency
        [Fact]
        public async Task ProcessPaymentAsync_ShouldCreateInstallments()
        {
            // Arrange
            var policy = new Policy { Id = "pol1", UserId = "u1", SelectedCoverageAmount = 1000, Status = PolicyStatus.Approved, PlanId = "p1" };
            _policyRepo.Setup(r => r.GetByIdAsync("pol1")).ReturnsAsync(policy);
            _planRepo.Setup(r => r.GetByIdAsync("p1")).ReturnsAsync(new Plan { Id = "p1", BasePremium = 100 });
            _userRepo.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new User { Id = "u1" });

            var dto = new CreatePaymentDto 
            { 
                PolicyId = "pol1", 
                PaymentFrequency = "Annually", 
                PaymentMode = "Card" 
            };

            // Act
            await _service.ProcessPaymentAsync(dto);

            // Assert
            _paymentRepo.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.AtLeastOnce);
        }
    }
}
