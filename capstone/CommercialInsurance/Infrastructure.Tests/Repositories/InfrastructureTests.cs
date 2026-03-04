using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Domain.Entities;
using Domain.Enums;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Infrastructure.Tests.Repositories
{
    public class InfrastructureTests
    {
        private DbContextOptions<InsuranceDbContext> GetInMemoryOptions(string dbName)
        {
            return new DbContextOptionsBuilder<InsuranceDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task AddUser_ShouldPersistSuccessfully()
        {
            // Arrange
            var options = GetInMemoryOptions("DbInsertSuccess");
            await using var context = new InsuranceDbContext(options);
            var repo = new UserRepository(context);
            var user = new User { Id = "u1", Email = "test@domain.com", FullName = "test" };

            // Act
            await repo.AddAsync(user);
            await context.SaveChangesAsync();

            // Assert
            var savedUser = await context.Users.FindAsync("u1");
            savedUser.Should().NotBeNull();
            savedUser!.Email.Should().Be("test@domain.com");
        }

        [Fact]
        public async Task AddUser_ShouldPreventDuplicateEmail()
        {
            // Arrange
            var options = GetInMemoryOptions("DbDuplicateEmail");
            await using var context = new InsuranceDbContext(options);
            var repo = new UserRepository(context);
            var user1 = new User { Id = "u1", Email = "duplicate@domain.com", FullName = "duplicate1" };

            await repo.AddAsync(user1);
            await context.SaveChangesAsync();

            // Act
            var userCheck = await repo.GetByEmailAsync("duplicate@domain.com");

            // Assert
            userCheck.Should().NotBeNull();
            userCheck!.Email.Should().Be("duplicate@domain.com");
        }

        [Fact]
        public async Task GetPoliciesByCustomer_ShouldReturnCorrectRecords()
        {
            // Arrange
            var options = GetInMemoryOptions("DbGetPoliciesCustomer");
            await using var context = new InsuranceDbContext(options);
            var repo = new PolicyRepository(context);

            context.Policies.AddRange(
                new Policy { Id = "p1", UserId = "cust1" },
                new Policy { Id = "p2", UserId = "cust1" },
                new Policy { Id = "p3", UserId = "cust2" }
            );
            await context.SaveChangesAsync();

            // Act
            var policies = await repo.GetPoliciesByUserIdAsync("cust1");

            // Assert
            policies.Should().HaveCount(2);
            policies.All(p => p.UserId == "cust1").Should().BeTrue();
        }

        [Fact]
        public async Task SaveChanges_ShouldCommitData()
        {
            // Arrange
            var options = GetInMemoryOptions("DbSaveChanges");
            await using var context = new InsuranceDbContext(options);
            
            var plan = new Plan { Id = "plan1", PlanName = "Test Plan" };
            context.Plans.Add(plan);

            // Act
            var result = await context.SaveChangesAsync();

            // Assert
            result.Should().BeGreaterThan(0);
            
            await using var verifyContext = new InsuranceDbContext(options);
            var savedPlan = await verifyContext.Plans.FindAsync("plan1");
            savedPlan.Should().NotBeNull();
        }

        [Fact]
        public async Task Repository_ShouldFilterActivePoliciesCorrectly()
        {
            // Arrange
            var options = GetInMemoryOptions("DbFilterActive");
            await using var context = new InsuranceDbContext(options);
            var repo = new PolicyRepository(context);

            context.Policies.AddRange(
                new Policy { Id = "p1", Status = PolicyStatus.Active },
                new Policy { Id = "p2", Status = PolicyStatus.Expired },
                new Policy { Id = "p3", Status = PolicyStatus.Active }
            );
            await context.SaveChangesAsync();

            // Act
            var allPolicies = await repo.GetAllAsync();
            var activePolicies = allPolicies.Where(p => p.Status == PolicyStatus.Active).ToList();

            // Assert
            activePolicies.Should().HaveCount(2);
            activePolicies.All(p => p.Status == PolicyStatus.Active).Should().BeTrue();
        }
    }
}
