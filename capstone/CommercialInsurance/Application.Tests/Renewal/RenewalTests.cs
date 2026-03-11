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
using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading.Tasks;

namespace Application.Tests.Renewal
{
    public class RenewalTests
    {
        // Asserts that renewing an expired policy successfully generates a new linked policy record and persists it via the repository
        [Fact]
        public async Task RenewPolicy_ShouldCreateNewPolicyAndLinkToPrevious()
        {
            // Arrange
            var policyRepoMock = new Mock<IPolicyRepository>();
            var notifyMock = new Mock<INotificationService>();
            
            var oldPolicy = new Policy 
            { 
                Id = "policy-abc", 
                Status = PolicyStatus.Expired, 
                EndDate = DateTime.UtcNow.AddDays(-1),
                PlanId = "plan1",
                UserId = "user1",
                PremiumAmount = 1000m
            };

            policyRepoMock.Setup(r => r.GetByIdAsync("policy-abc"))
                          .ReturnsAsync(oldPolicy);
            
            // Mocking GetByIdAsync for new policy to return something for the return statement
            policyRepoMock.Setup(r => r.GetByIdAsync(It.Is<string>(id => id != "policy-abc")))
                          .ReturnsAsync(new Policy { Id = "new-policy", PreviousPolicyId = "policy-abc" });

            var policyService = new PolicyService(
                policyRepoMock.Object, 
                new Mock<IPlanRepository>().Object, 
                new Mock<IUserRepository>().Object, 
                new Mock<IBusinessProfileRepository>().Object, 
                new Mock<IGenericRepository<Document>>().Object, 
                new Mock<IClaimRepository>().Object, 
                new Mock<IPaymentRepository>().Object, 
                new Mock<IGenericRepository<ClaimHistoryLog>>().Object, 
                new Mock<IWebHostEnvironment>().Object, 
                notifyMock.Object);

            // Act
            var result = await policyService.RenewPolicyAsync("policy-abc");

            // Assert
            result.Should().NotBeNull();
            
            // Verifying AddAsync was called with a linked policy
            policyRepoMock.Verify(r => r.AddAsync(It.Is<Policy>(p => p.PreviousPolicyId == "policy-abc")), Times.Once);
        }

        // Validates that attempting to renew a policy too far from its expiry date safely catches the violation and throws an exception
        [Fact]
        public async Task RenewPolicy_ShouldThrowException_WhenActivePolicyExists()
        {
            // Arrange
            var policyRepoMock = new Mock<IPolicyRepository>();
            
            var activeOldPolicy = new Policy 
            { 
                Id = "policy-xyz", 
                Status = PolicyStatus.Active, 
                // Expiry is far in the future
                EndDate = DateTime.UtcNow.AddDays(90)
            };

            policyRepoMock.Setup(r => r.GetByIdAsync("policy-xyz"))
                          .ReturnsAsync(activeOldPolicy);
            
            var policyService = new PolicyService(
                policyRepoMock.Object, 
                new Mock<IPlanRepository>().Object, 
                new Mock<IUserRepository>().Object, 
                new Mock<IBusinessProfileRepository>().Object, 
                new Mock<IGenericRepository<Document>>().Object, 
                new Mock<IClaimRepository>().Object, 
                new Mock<IPaymentRepository>().Object, 
                new Mock<IGenericRepository<ClaimHistoryLog>>().Object, 
                new Mock<IWebHostEnvironment>().Object, 
                new Mock<INotificationService>().Object);

            // Act
            Func<Task> act = async () => await policyService.RenewPolicyAsync("policy-xyz");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("*Renewal is available 30 days before expiry*");
        }
    }
}
