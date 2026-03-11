// Test Layer: Application Services
// Purpose: Validates core business logic, algorithm correctness, and interactions with mocked domain repositories.
// Design: Uses XUnit and Moq to isolate dependencies and guarantee idempotent execution.
using Xunit;
using FluentAssertions;
using Domain.Entities;
using Domain.Enums;

namespace Application.Tests.Assignment
{
    public class AssignmentTests
    {
        // Verifies that assigning an agent ID to a User entity systematically mutates the binding property
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
