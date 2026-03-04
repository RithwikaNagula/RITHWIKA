using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface ISupportInquiryService
    {
        Task<SupportInquiryDto> CreateInquiryAsync(CreateSupportInquiryDto dto);
        Task<IEnumerable<SupportInquiryDto>> GetAllInquiriesAsync();
        Task MarkAsResolvedAsync(string id);
    }
}
