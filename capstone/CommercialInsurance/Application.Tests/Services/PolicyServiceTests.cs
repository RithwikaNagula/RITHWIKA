using Xunit;
using Moq;
using FluentAssertions;
using Application.Services;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Application.Tests.Services
{
    public class PolicyServiceTests
    {
        [Fact]
        public async Task CreatePolicy_ShouldPreventDuplicateActivePolicy()
        {
            // Arrange
            var policyRepoMock = new Mock<IPolicyRepository>();
            var profileRepoMock = new Mock<IBusinessProfileRepository>();
            var planRepoMock = new Mock<IPlanRepository>();
            var userRepoMock = new Mock<IUserRepository>();

            var businessProfile = new BusinessProfile { Id = "prof1", UserId = "user1", IsProfileCompleted = true };
            profileRepoMock.Setup(r => r.GetByIdAsync("prof1")).ReturnsAsync(businessProfile);
            
            var plan = new Plan { Id = "plan1" };
            planRepoMock.Setup(r => r.GetByIdAsync("plan1")).ReturnsAsync(plan);

            var user = new User { Id = "user1" };
            userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

            var activePolicies = new List<Policy> 
            {
                new Policy { UserId = "user1", Status = PolicyStatus.Active, PlanId = "plan1", BusinessProfileId = "prof1" }
            };
            
            policyRepoMock.Setup(r => r.GetPoliciesByUserIdAsync("user1"))
                          .ReturnsAsync(activePolicies);

            var policyService = new PolicyService(
                policyRepoMock.Object, 
                planRepoMock.Object, 
                userRepoMock.Object, 
                profileRepoMock.Object, 
                new Mock<IGenericRepository<Document>>().Object, 
                new Mock<IClaimRepository>().Object, 
                new Mock<IPaymentRepository>().Object, 
                new Mock<IGenericRepository<ClaimHistoryLog>>().Object, 
                new Mock<IWebHostEnvironment>().Object, 
                new Mock<INotificationService>().Object);

            var dto = new CreatePolicyDto { PlanId = "plan1", BusinessProfileId = "prof1" };

            // Act
            Func<Task> act = async () => await policyService.CreatePolicyAsync(dto, "user1", null!);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("*already has an active or pending policy*"); 
        }
    }
}
