// DTO for customer support inquiries submitted via the contact form, including name, email, subject, and message.
using System;

namespace Application.DTOs
{
    // Inbound DTO for the public contact form; does not require authentication
    public class CreateSupportInquiryDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    // Outbound DTO representing a stored inquiry with its resolution status and timestamp
    public class SupportInquiryDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
    }
}
