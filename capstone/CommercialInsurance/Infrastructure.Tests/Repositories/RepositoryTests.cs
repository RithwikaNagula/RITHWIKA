// Test Layer: Infrastructure Data Access
// Purpose: Ensures Entity Framework Core database contexts and repository abstract queries compile and execute correctly against in-memory DBs.
// Design: Uses XUnit and Moq to isolate dependencies and guarantee idempotent execution.
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Domain.Entities;
using Domain.Enums;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Infrastructure.Tests.Repositories
{
    public class RepositoryTests
    {
        private DbContextOptions<InsuranceDbContext> GetInMemoryOptions(string dbName)
        {
            return new DbContextOptionsBuilder<InsuranceDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        // Verifies that the generic repository implementation can successfully insert and fetch an InsuranceType
        [Fact]
        public async Task InsuranceTypeRepository_ShouldCreateAndRetrieve()
        {
            var options = GetInMemoryOptions(Guid.NewGuid().ToString());
            await using var context = new InsuranceDbContext(options);
            var repo = new InsuranceTypeRepository(context);

            var type = new InsuranceType { Id = "t1", TypeName = "Test Type" };
            await repo.AddAsync(type);
            await context.SaveChangesAsync();

            var retrieved = await repo.GetByIdAsync("t1");
            retrieved.Should().NotBeNull();
            retrieved.TypeName.Should().Be("Test Type");
        }

        // Ensures that a new Claim entity is properly tracked and persisted by the repository context
        [Fact]
        public async Task ClaimRepository_ShouldAddRecordCorrectly()
        {
            var options = GetInMemoryOptions(Guid.NewGuid().ToString());
            await using var context = new InsuranceDbContext(options);
            var repo = new ClaimRepository(context);

            var claim = new Claim { Id = "c1", PolicyId = "p1", ClaimAmount = 5000 };
            await repo.AddAsync(claim);
            await context.SaveChangesAsync();

            var retrieved = await repo.GetByIdAsync("c1");
            retrieved.Should().NotBeNull();
            retrieved.ClaimAmount.Should().Be(5000);
        }

        // Confirms that the generic UpdateAsync method successfully applies modifications to an existing Plan
        [Fact]
        public async Task PlanRepository_ShouldUpdateSuccessfully()
        {
            var options = GetInMemoryOptions(Guid.NewGuid().ToString());
            await using var context = new InsuranceDbContext(options);
            var repo = new PlanRepository(context);

            var plan = new Plan { Id = "pl1", PlanName = "Old Plan" };
            await repo.AddAsync(plan);

            plan.PlanName = "New Plan";
            await repo.UpdateAsync(plan);

            var updated = await repo.GetByIdAsync("pl1");
            updated!.PlanName.Should().Be("New Plan");
        }

        // Verifies that mutating a Notification entity's property and calling UpdateAsync persists the change
        [Fact]
        public async Task NotificationRepository_ShouldMarkAsRead()
        {
            var options = GetInMemoryOptions(Guid.NewGuid().ToString());
            await using var context = new InsuranceDbContext(options);
            var repo = new NotificationRepository(context);

            var notification = new Notification { Id = "n1", Message = "Test", IsRead = false };
            await repo.AddAsync(notification);

            notification.IsRead = true;
            await repo.UpdateAsync(notification);

            var updated = await repo.GetByIdAsync("n1");
            updated!.IsRead.Should().BeTrue();
        }
    }
}
