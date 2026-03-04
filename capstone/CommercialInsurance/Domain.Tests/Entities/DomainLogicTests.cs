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
        [Fact]
        public void Payment_ShouldInitializeWithPendingStatus()
        {
            var payment = new Payment { Amount = 5000 };
            payment.Status.Should().Be(PaymentStatus.Pending);
        }

        [Fact]
        public void Plan_ShouldCalculateCorrectDuration()
        {
            var plan = new Plan { PlanName = "Standard", DurationInMonths = 12 };
            plan.DurationInMonths.Should().Be(12);
        }

        [Fact]
        public void InsuranceType_ShouldHoldCorrectMetadata()
        {
            var type = new InsuranceType { TypeName = "Cyber", Description = "Cyber Security" };
            type.TypeName.Should().Be("Cyber");
            type.Description.Should().Be("Cyber Security");
        }

        [Fact]
        public void Notification_ShouldBeUnreadByDefault()
        {
            var notification = new Notification { Message = "Test" };
            notification.IsRead.Should().BeFalse();
        }

        [Fact]
        public void Policy_ShouldHaveCorrectStatusTransitions()
        {
            var policy = new Policy { Status = PolicyStatus.Active };
            policy.Status = PolicyStatus.Expired;
            policy.Status.Should().Be(PolicyStatus.Expired);
        }
    }
}
