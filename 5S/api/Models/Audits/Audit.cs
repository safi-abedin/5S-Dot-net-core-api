using api.Models.Base;

namespace api.Models.Audits
{
    public class Audit : BaseEntity
    {
        public int ZoneId { get; set; }

        public string AuditorName { get; set; }
        public string AuditeeName { get; set; }
        public string Department { get; set; }

        public DateTime AuditDate { get; set; } = DateTime.UtcNow;

        public decimal TotalScore { get; set; }
        public decimal Percentage { get; set; }

        public string Status { get; set; }

        public List<AuditItem> Items { get; set; }
    }
}
