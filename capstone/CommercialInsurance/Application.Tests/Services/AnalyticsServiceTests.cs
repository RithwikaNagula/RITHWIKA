// Test Layer: Application Services
// Purpose: Validates core business logic, algorithm correctness, and interactions with mocked domain repositories.
// Design: Uses XUnit and Moq to isolate dependencies and guarantee idempotent execution.
using Xunit;
using Moq;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Services;
using Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using System;
using Application.Interfaces.Services;
using Application.DTOs;

namespace Application.Tests.Services
{
    public class AnalyticsServiceTests
    {
        private readonly InsuranceDbContext _context;
        private readonly AnalyticsService _service;

        public AnalyticsServiceTests()
        {
            var options = new DbContextOptionsBuilder<InsuranceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new InsuranceDbContext(options);
            _service = new AnalyticsService(_context);
        }

        // Verifies that the analytics service properly constructs and returns global claim statistics without crashing on empty data sets
        [Fact]
        public async Task GetClaimsPerformanceAsync_ShouldReturnStats()
        {
            // Act
            var result = await _service.GetClaimsPerformanceAsync();

            // Assert
            result.Should().NotBeNull();
            result.TotalClaims.Should().Be(0);
        }
    }
}
