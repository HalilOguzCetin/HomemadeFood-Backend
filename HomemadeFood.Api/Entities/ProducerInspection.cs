using HomemadeFood.Api.Constants;

namespace HomemadeFood.Api.Entities
{
    public class ProducerInspection
    {
        public int Id { get; set; }

        public int ProducerProfileId { get; set; }
        public ProducerProfile ProducerProfile { get; set; } = null!;

        public string AuthorityType { get; set; } =
            ProducerInspectionAuthorityTypes.PlatformInternal;

        public string InspectionType { get; set; } = "Other";

        public DateTime InspectionDate { get; set; } = DateTime.UtcNow;

        public string Result { get; set; } =
            ProducerInspectionResults.Pending;

        public string? Notes { get; set; }

        /*
         * İleride denetim tutanağı/fotoğrafı private storage'da tutulursa
         * burada yalnız güvenli referans bulunur.
         */
        public string? DocumentReference { get; set; }

        public int? RecordedByAdminId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
