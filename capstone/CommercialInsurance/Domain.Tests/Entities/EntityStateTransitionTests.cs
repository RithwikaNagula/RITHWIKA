// Test Layer: Domain Entities
// Purpose: Asserts intrinsic behavior and state mutations isolated within the raw business entities.
// Design: Uses XUnit and Moq to isolate dependencies and guarantee idempotent execution.
using Xunit;
using FluentAssertions;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using System;

namespace Domain.Tests.Entities
{
    public class PolicyTests
    {
        // Verifies the correct assignment and retrieval of the policy's valid date interval bounds
        [Fact]
        public void Policy_ShouldSetDatesProperly()
        {
            // Arrange
            var startDate = DateTime.UtcNow;
            var policy = new Policy { StartDate = startDate, EndDate = startDate.AddDays(10) };

            // Act & Assert
            policy.StartDate.Should().Be(startDate);
            policy.EndDate.Should().Be(startDate.AddDays(10));
        }
    }

    public class ClaimTests
    {
        // Confirms that a newly instantiated Claim sets its initial lifecycle state to Submitted
        [Fact]
        public void Claim_ShouldHaveSubmittedStatusByDefault()
        {
            // Arrange
            var claim = new Claim();

            // Act & Assert
            claim.Status.Should().Be(ClaimStatus.Submitted);
        }

        // Verifies that the Status property on a Claim is mutable and can be advanced by an officer
        [Fact]
        public void Claim_CanUpdateStatus()
        {
            // Arrange
            var claim = new Claim { Status = ClaimStatus.Submitted };

            // Act
            claim.Status = ClaimStatus.Approved;

            // Assert
            claim.Status.Should().Be(ClaimStatus.Approved);
        }
    }

    public class BusinessRuleExceptionTests
    {
        // Ensures the custom domain exception properly routes its specific message to the base Exception 
        [Fact]
        public void BusinessRuleException_ShouldStoreCorrectMessage()
        {
            // Arrange
            var ex = new BusinessRuleException("Test exception message");

            // Act & Assert
            ex.Message.Should().Be("Test exception message");
        }
    }

    public class UserTests
    {
        // Verifies that a newly created User entity defaults to the first Enum value
        [Fact]
        public void User_ShouldInitializeWithDefaultRole()
        {
            // Arrange
            var user = new User();

            // Act & Assert
            user.Role.Should().Be(UserRole.Admin);
        }
    }
}
