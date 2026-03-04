using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Application.Interfaces.Services;
using Application.DTOs;
using API.Controllers;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Tests.Controllers
{
    public class EndpointTests
    {
        private void SetupUser(ControllerBase controller, string userId, string role = "Customer")
        {
            var claims = new[] 
            { 
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task InsuranceType_GetAll_ShouldReturnOk()
        {
            var serviceMock = new Mock<IInsuranceTypeService>();
            serviceMock.Setup(s => s.GetAllInsuranceTypesAsync()).ReturnsAsync(new List<InsuranceTypeDto>());
            var controller = new InsuranceTypeController(serviceMock.Object);

            var result = await controller.GetAll();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Claim_Submit_ShouldReturnOk()
        {
            var serviceMock = new Mock<IClaimService>();
            serviceMock.Setup(s => s.FileClaimAsync(It.IsAny<CreateClaimDto>(), It.IsAny<string>(), It.IsAny<IEnumerable<IFormFile>>()))
                       .ReturnsAsync(new ClaimDto { Id = "clm-1" });
            var controller = new ClaimController(serviceMock.Object);
            SetupUser(controller, "user-123");

            var dto = new CreateClaimDto { ClaimAmount = 500, Description = "Test" };
            var result = await controller.FileClaim("pol-1", dto, new List<IFormFile>());

            result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task Plan_GetAll_ShouldReturnOk()
        {
            var serviceMock = new Mock<IPlanService>();
            serviceMock.Setup(s => s.GetAllPlansAsync()).ReturnsAsync(new List<PlanDto>());
            var controller = new PlanController(serviceMock.Object);

            var result = await controller.GetAll();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Notification_GetUnread_ShouldReturnOk()
        {
            var serviceMock = new Mock<INotificationService>();
            serviceMock.Setup(s => s.GetUserNotificationsAsync(It.IsAny<string>()))
                       .ReturnsAsync(new List<NotificationDto>());
            var controller = new NotificationsController(serviceMock.Object);
            SetupUser(controller, "user-123");

            var result = await controller.GetMyNotifications();

            result.Result.Should().BeOfType<OkObjectResult>();
        }
    }
}
