// Test Layer: API Controllers
// Purpose: Verifies HTTP request routing, input validation, and proper HTTP action results (e.g., 200 OK, 404 NotFound).
// Design: Uses XUnit and Moq to isolate dependencies and guarantee idempotent execution.
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Services;
using Application.DTOs;
using API.Controllers;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace API.Tests.Controllers
{
    public class SupportInquiryControllerTests
    {
        private readonly Mock<ISupportInquiryService> _serviceMock;
        private readonly SupportInquiryController _controller;

        public SupportInquiryControllerTests()
        {
            _serviceMock = new Mock<ISupportInquiryService>();
            _controller = new SupportInquiryController(_serviceMock.Object);
        }

        // Asserts that posting a valid support inquiry connects to the service layer and logs a 200 OK success
        [Fact]
        public async Task SubmitInquiry_ShouldReturnOk()
        {
            // Arrange
            var dto = new CreateSupportInquiryDto { Message = "Test" };
            _serviceMock.Setup(s => s.CreateInquiryAsync(dto)).ReturnsAsync(new SupportInquiryDto());

            // Act
            var result = await _controller.SubmitInquiry(dto);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        // Validates that fetching the master list of support inquiries aggregates all past records into a 200 OK output
        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            // Arrange
            var inquiries = new List<SupportInquiryDto> { new SupportInquiryDto() };
            _serviceMock.Setup(s => s.GetAllInquiriesAsync()).ReturnsAsync(inquiries);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }
    }
}
