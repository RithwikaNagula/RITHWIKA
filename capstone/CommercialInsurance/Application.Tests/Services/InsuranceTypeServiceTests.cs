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
    public class InsuranceTypeServiceTests
    {
        // Verifies that fetching the complete catalog successfully accesses the repository and maps all active entities to DTOs
        [Fact]
        public async Task GetAllInsuranceTypesAsync_ShouldReturnAllTypes()
        {
            // Arrange
            var repoMock = new Mock<IInsuranceTypeRepository>();
            var types = new List<InsuranceType> 
            { 
                new InsuranceType { Id = "1", TypeName = "General Liability" },
                new InsuranceType { Id = "2", TypeName = "Professional Liability" }
            };
            repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(types);

            var service = new InsuranceTypeService(repoMock.Object);

            // Act
            var result = await service.GetAllInsuranceTypesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(t => t.TypeName == "General Liability");
        }

        // Asserts that a targeted primary key lookup accurately returns a specifically mapped catalog entity
        [Fact]
        public async Task GetInsuranceTypeByIdAsync_ShouldReturnCorrectType()
        {
            // Arrange
            var repoMock = new Mock<IInsuranceTypeRepository>();
            var type = new InsuranceType { Id = "1", TypeName = "General Liability" };
            repoMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(type);

            var service = new InsuranceTypeService(repoMock.Object);

            // Act
            var result = await service.GetInsuranceTypeByIdAsync("1");

            // Assert
            result.Should().NotBeNull();
            result!.TypeName.Should().Be("General Liability");
        }

        // Confirms that passing a simplified creation DTO cleanly converts into a domain entity and commits it
        [Fact]
        public async Task CreateInsuranceTypeAsync_ShouldInvokeRepository()
        {
            // Arrange
            var repoMock = new Mock<IInsuranceTypeRepository>();
            var service = new InsuranceTypeService(repoMock.Object);
            var dto = new CreateInsuranceTypeDto { TypeName = "New Type" };

            // Act
            await service.CreateInsuranceTypeAsync(dto);

            // Assert
            repoMock.Verify(r => r.AddAsync(It.IsAny<InsuranceType>()), Times.Once);
        }
    }
}
