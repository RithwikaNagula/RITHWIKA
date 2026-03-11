// Stores metadata (file path, type) for uploaded documents attached to policies or claims.
namespace Domain.Entities
{
    public class Document
    {
        public string Id { get; set; } = string.Empty;

        // A document can be linked to either a policy or a claim (or both)
        public string? PolicyId { get; set; }
        public string? ClaimId { get; set; }

        // Original filename as uploaded by the user
        public string FileName { get; set; } = string.Empty;
        // Server-side path under wwwroot/uploads where the file is stored
        public string FilePath { get; set; } = string.Empty;
        // MIME type or file extension (e.g., application/pdf, image/jpeg)
        public string FileType { get; set; } = string.Empty;
        // File size in bytes; used for display and upload validation
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Policy? Policy { get; set; }
        public Claim? Claim { get; set; }
    }
}
