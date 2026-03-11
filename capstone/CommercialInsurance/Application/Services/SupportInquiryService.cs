// Saves customer support inquiries from the contact form and provides admin listing with basic filtering.
using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class SupportInquiryService : ISupportInquiryService
    {
        private readonly IGenericRepository<SupportInquiry> _repository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;

        // Requires the NotificationService alongside pure CRUD repositories so it can alert admins 
        // to incoming customer support requests instantly.
        public SupportInquiryService(
            IGenericRepository<SupportInquiry> repository,
            IUserRepository userRepository,
            INotificationService notificationService)
        {
            _repository = repository;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        // Writes a new contact inquiry to the database and dispatches a system-wide broadcast 
        // to every user with the pure 'Admin' role via SignalR.
        public async Task<SupportInquiryDto> CreateInquiryAsync(CreateSupportInquiryDto dto)
        {
            var inquiry = new SupportInquiry
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Message = dto.Message,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(inquiry);

            // Fetch all admin accounts so they can all be notified in the dashboard
            var admins = await _userRepository.FindAsync(u => u.Role == UserRole.Admin);
            foreach (var admin in admins)
            {
                await _notificationService.CreateNotificationAsync(
                    admin.Id,
                    "New Support Inquiry",
                    $"New inquiry from {dto.FullName} ({dto.Email})",
                    NotificationType.System.ToString(),
                    inquiry.Id
                );
            }

            return MapToDto(inquiry);
        }

        // Pulls all historical contact form messages, sorted chronologically descending.
        public async Task<IEnumerable<SupportInquiryDto>> GetAllInquiriesAsync()
        {
            var inquiries = await _repository.GetAllAsync();
            return inquiries.OrderByDescending(i => i.CreatedAt).Select(MapToDto);
        }

        // Updates the boolean resolution flag natively avoiding broad entity reconstruction
        public async Task MarkAsResolvedAsync(string id)
        {
            var inquiry = await _repository.GetByIdAsync(id);
            if (inquiry != null)
            {
                inquiry.IsResolved = true;
                await _repository.UpdateAsync(inquiry);
            }
        }

        // Local mapper standardizing what fields are permitted over the wire via the API.
        private static SupportInquiryDto MapToDto(SupportInquiry inquiry) => new()
        {
            Id = inquiry.Id,
            FullName = inquiry.FullName,
            Email = inquiry.Email,
            Message = inquiry.Message,
            CreatedAt = inquiry.CreatedAt,
            IsResolved = inquiry.IsResolved
        };
    }
}
