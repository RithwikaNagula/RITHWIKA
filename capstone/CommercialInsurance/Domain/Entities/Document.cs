// Provides core functionality and structures for the application.
namespace Domain.Entities
{
    public class Document
    {
        public string Id { get; set; } = string.Empty;

        // Optional links
        public string? PolicyId { get; set; }
        public string? ClaimId { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Policy? Policy { get; set; }
        public Claim? Claim { get; set; }
    }
}
