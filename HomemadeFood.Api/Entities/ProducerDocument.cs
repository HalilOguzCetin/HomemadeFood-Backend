using HomemadeFood.Api.Constants;

namespace HomemadeFood.Api.Entities
{
    public class ProducerDocument
    {
        public int Id { get; set; }

        public int ProducerProfileId { get; set; }
        public ProducerProfile ProducerProfile { get; set; } = null!;

        public string DocumentType { get; set; } =
            ProducerDocumentTypes.Other;

        public string? DocumentNumber { get; set; }

        /*
         * Dokümanın kendisi DB'ye gömülmez. İleride private object storage
         * kullanıldığında yalnız güvenli/opaque storage reference tutulur.
         */
        public string? StorageReference { get; set; }

        public string? OriginalFileName { get; set; }

        public string VerificationStatus { get; set; } =
            ProducerDocumentVerificationStatuses.NotVerified;

        public DateTime? IssuedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public int? ReviewedByAdminId { get; set; }

        public string? ReviewNote { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
