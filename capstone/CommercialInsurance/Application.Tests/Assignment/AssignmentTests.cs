using Xunit;
using FluentAssertions;
using Domain.Entities;
using Domain.Enums;

namespace Application.Tests.Assignment
{
    public class AssignmentTests
    {
        [Fact]
        public void User_ShouldStoreAssignedAgentCorrectly()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "test@domain.com", Role = UserRole.Customer };
            
            // Act
            user.AssignedAgentId = "agent-x";
            
            // Assert
            user.AssignedAgentId.Should().Be("agent-x");
        }
    }
}
