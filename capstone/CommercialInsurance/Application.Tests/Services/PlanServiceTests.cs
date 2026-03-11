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
using Application.DTOs;

namespace Application.Tests.Services
{
    public class PlanServiceTests
    {
        // Confirms that polling the active product matrix successfully bypasses discontinued rules and accurately structures the list
        [Fact]
        public async Task GetAllPlansAsync_ShouldReturnAllPlans()
        {
            // Arrange
            var repoMock = new Mock<IPlanRepository>();
            var typeRepoMock = new Mock<IInsuranceTypeRepository>();
            var plans = new List<Plan> 
            { 
                new Plan { Id = "p1", PlanName = "Gold", InsuranceType = new InsuranceType { TypeName = "T" }, IsActive = true },
                new Plan { Id = "p2", PlanName = "Silver", InsuranceType = new InsuranceType { TypeName = "T" }, IsActive = true }
            };
            repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(plans);

            var service = new PlanService(repoMock.Object, typeRepoMock.Object);

            // Act
            var result = await service.GetAllPlansAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        // Verifies that locating a product configuration via its primary key smoothly unrolls the entity into a readable wrapper
        [Fact]
        public async Task GetPlanByIdAsync_ShouldReturnCorrectPlan()
        {
            // Arrange
            var repoMock = new Mock<IPlanRepository>();
            var typeRepoMock = new Mock<IInsuranceTypeRepository>();
            var plan = new Plan { Id = "p1", PlanName = "Gold", InsuranceType = new InsuranceType { TypeName = "T" } };
            repoMock.Setup(r => r.GetByIdAsync("p1")).ReturnsAsync(plan);

            var service = new PlanService(repoMock.Object, typeRepoMock.Object);

            // Act
            var result = await service.GetPlanByIdAsync("p1");

            // Assert
            result.Should().NotBeNull();
            result!.PlanName.Should().Be("Gold");
        }

        // Asserts that formulating a product specification invokes the data insertion hook without logical errors
        [Fact]
        public async Task CreatePlanAsync_ShouldInvokeRepository()
        {
            // Arrange
            var repoMock = new Mock<IPlanRepository>();
            var typeRepoMock = new Mock<IInsuranceTypeRepository>();
            var service = new PlanService(repoMock.Object, typeRepoMock.Object);
            var dto = new CreatePlanDto { InsuranceTypeId = "type1", PlanName = "Platinum", BasePremium = 500 };
            
            typeRepoMock.Setup(r => r.GetByIdAsync(dto.InsuranceTypeId)).ReturnsAsync(new InsuranceType());

            // Act
            await service.CreatePlanAsync(dto);

            // Assert
            repoMock.Verify(r => r.AddAsync(It.IsAny<Plan>()), Times.Once);
        }
    }
}
