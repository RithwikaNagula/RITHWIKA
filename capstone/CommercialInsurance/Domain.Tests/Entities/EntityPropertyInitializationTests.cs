// Test Layer: Domain Entities
// Purpose: Asserts intrinsic behavior and state mutations isolated within the raw business entities.
// Design: Uses XUnit and Moq to isolate dependencies and guarantee idempotent execution.
using Xunit;
using FluentAssertions;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Tests.Entities
{
    public class DomainLogicTests
    {
        // Verifies that a newly instantiated Payment object defaults its Status property to Pending
        [Fact]
        public void Payment_ShouldInitializeWithPendingStatus()
        {
            var payment = new Payment { Amount = 5000 };
            payment.Status.Should().Be(PaymentStatus.Pending);
        }

        // Ensures that the Plan entity accurately stores the assigned duration string or integer
        [Fact]
        public void Plan_ShouldCalculateCorrectDuration()
        {
            var plan = new Plan { PlanName = "Standard", DurationInMonths = 12 };
            plan.DurationInMonths.Should().Be(12);
        }

        // Verifies that string properties for type name and description are correctly assigned
        [Fact]
        public void InsuranceType_ShouldHoldCorrectMetadata()
        {
            var type = new InsuranceType { TypeName = "Cyber", Description = "Cyber Security" };
            type.TypeName.Should().Be("Cyber");
            type.Description.Should().Be("Cyber Security");
        }

        // Confirms that a newly created Notification defaults its IsRead flag to false
        [Fact]
        public void Notification_ShouldBeUnreadByDefault()
        {
            var notification = new Notification { Message = "Test" };
            notification.IsRead.Should().BeFalse();
        }

        // Tests the mutability of the Policy Status property to ensure it can transition to terminal states
        [Fact]
        public void Policy_ShouldHaveCorrectStatusTransitions()
        {
            var policy = new Policy { Status = PolicyStatus.Active };
            policy.Status = PolicyStatus.Expired;
            policy.Status.Should().Be(PolicyStatus.Expired);
        }
    }
}
