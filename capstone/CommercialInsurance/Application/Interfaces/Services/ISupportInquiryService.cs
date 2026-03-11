// Service contract for submitting and listing customer support inquiries.
using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface ISupportInquiryService
    {
        // Stores a new support inquiry from the public contact form
        Task<SupportInquiryDto> CreateInquiryAsync(CreateSupportInquiryDto dto);
        // Returns all inquiries for the admin support panel, ordered by recency
        Task<IEnumerable<SupportInquiryDto>> GetAllInquiriesAsync();
        // Marks an inquiry as resolved so it can be filtered out of the active queue
        Task MarkAsResolvedAsync(string id);
    }
}
