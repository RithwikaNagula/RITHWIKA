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
        [Fact]
        public void Claim_ShouldHaveSubmittedStatusByDefault()
        {
            // Arrange
            var claim = new Claim();

            // Act & Assert
            claim.Status.Should().Be(ClaimStatus.Submitted);
        }

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
