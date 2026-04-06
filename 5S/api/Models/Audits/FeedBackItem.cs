using api.Models.Base;

namespace api.Models.Audits
{
    public class FeedBackItem : BaseEntity
    {
        public int AuditId { get; set; }
        public Audit Audit { get; set; }

        public string? Comment { get; set; }

        public List<string>? ImageUrls { get; set; }

        public bool? Good { get; set; } = true;

        public bool? Bad { get; set; }=false;
    }
}
