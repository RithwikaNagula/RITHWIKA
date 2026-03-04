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
