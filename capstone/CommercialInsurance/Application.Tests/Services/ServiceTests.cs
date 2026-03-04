using Xunit;
using Moq;
using FluentAssertions;
using Application.Services;
using Application.DTOs;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using Application.Hubs;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Application.Interfaces.Services;

namespace Application.Tests.Services
{
    public class ServiceTests
    {
        [Fact]
        public async Task InsuranceTypeService_GetAll_ShouldReturnList()
        {
            var repoMock = new Mock<IGenericRepository<InsuranceType>>();
            var types = new List<InsuranceType> { new InsuranceType { Id = "1", TypeName = "Test", CreatedAt = DateTime.UtcNow } };
            repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(types);

            var service = new InsuranceTypeService(repoMock.Object);
            var result = await service.GetAllInsuranceTypesAsync();

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task PlanService_GetById_ShouldReturnCorrectPlan()
        {
            var repoMock = new Mock<IPlanRepository>();
            var plan = new Plan { Id = "p1", PlanName = "Silver", CreatedAt = DateTime.UtcNow };
            repoMock.Setup(r => r.GetByIdAsync("p1")).ReturnsAsync(plan);

            var service = new PlanService(repoMock.Object);
            var result = await service.GetPlanByIdAsync("p1");

            result!.PlanName.Should().Be("Silver");
        }

        [Fact]
        public async Task NotificationService_Create_ShouldCallRepository()
        {
            var repoMock = new Mock<IGenericRepository<Notification>>();
            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<ISingleClientProxy>();

            hubContextMock.Setup(x => x.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(x => x.User(It.IsAny<string>())).Returns(clientProxyMock.Object);

            var service = new NotificationService(repoMock.Object, hubContextMock.Object);

            await service.CreateNotificationAsync("u1", "Title", "Message", "System");

            repoMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
        }

        [Fact]
        public async Task ClaimService_File_ShouldCallRepository()
        {
            var claimRepoMock = new Mock<IClaimRepository>();
            var policyRepoMock = new Mock<IPolicyRepository>();
            var userRepoMock = new Mock<IUserRepository>();
            var logRepoMock = new Mock<IGenericRepository<ClaimHistoryLog>>();
            var docRepoMock = new Mock<IGenericRepository<Document>>();
            var profileRepoMock = new Mock<IBusinessProfileRepository>();
            var envMock = new Mock<IWebHostEnvironment>();
            var notificationServiceMock = new Mock<INotificationService>();

            var service = new ClaimService(
                claimRepoMock.Object, 
                policyRepoMock.Object, 
                userRepoMock.Object, 
                logRepoMock.Object, 
                docRepoMock.Object,
                profileRepoMock.Object,
                envMock.Object,
                notificationServiceMock.Object);

            var policy = new Policy 
            { 
                Id = "pol1", 
                Status = PolicyStatus.Active, 
                SelectedCoverageAmount = 10000,
                UserId = "user1"
            };
            
            policyRepoMock.Setup(r => r.GetByIdAsync("pol1")).ReturnsAsync(policy);
            userRepoMock.Setup(r => r.GetByIdAsync("user1")).ReturnsAsync(new User { Id = "user1", AssignedClaimsOfficerId = "officer1" });
            claimRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Claim>());
            docRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Document>());
            logRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ClaimHistoryLog>());

            // For return value of FileClaimAsync which calls GetClaimByIdAsync
            claimRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                         .ReturnsAsync(new Claim { Id = "clm1", PolicyId = "pol1", ClaimAmount = 1000 });
                         
            var claimDto = new CreateClaimDto { ClaimAmount = 1000, Description = "Accident" };
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");
            var files = new List<IFormFile> { fileMock.Object };

            var result = await service.FileClaimAsync(claimDto, "pol1", files);

            claimRepoMock.Verify(r => r.AddAsync(It.IsAny<Claim>()), Times.Once);
            result.Should().NotBeNull();
        }
    }
}
