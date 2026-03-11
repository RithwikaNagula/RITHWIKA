// Test Layer: Application Services
// Purpose: Validates core business logic, algorithm correctness, and interactions with mocked domain repositories.
// Design: Uses XUnit and Moq to isolate dependencies and guarantee idempotent execution.
using Xunit;
using Moq;
using FluentAssertions;
using Application.Services;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Tests.Services
{
    public class SupportInquiryServiceTests
    {
        private readonly Mock<IGenericRepository<SupportInquiry>> _repo;
        private readonly Mock<IUserRepository> _userRepo;
        private readonly Mock<INotificationService> _notif;
        private readonly SupportInquiryService _service;

        public SupportInquiryServiceTests()
        {
            _repo = new Mock<IGenericRepository<SupportInquiry>>();
            _userRepo = new Mock<IUserRepository>();
            _notif = new Mock<INotificationService>();
            _service = new SupportInquiryService(_repo.Object, _userRepo.Object, _notif.Object);
        }

        // Confirms that retrieving the global inbox of customer messages correctly pulls and maps the data set
        [Fact]
        public async Task GetAllInquiriesAsync_ShouldReturnList()
        {
            // Arrange
            _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SupportInquiry> { new SupportInquiry() });

            // Act
            var result = await _service.GetAllInquiriesAsync();

            // Assert
            result.Should().HaveCount(1);
        }

        // Asserts that submitting a new inquiry payload effectively invokes an insert operation into the repository
        [Fact]
        public async Task CreateInquiryAsync_ShouldInvokeRepo()
        {
            // Arrange
            var dto = new CreateSupportInquiryDto { FullName = "N", Email = "e", Message = "M" };
            _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                     .ReturnsAsync(new List<User>());

            // Act
            await _service.CreateInquiryAsync(dto);

            // Assert
            _repo.Verify(r => r.AddAsync(It.IsAny<SupportInquiry>()), Times.Once);
        }
    }
}
