using api.Models.Base;
using api.Models.Checklists;

namespace api.Models.Audits
{
    public class AuditItem : BaseEntity
    {
        public int AuditId { get; set; }
        public Audit Audit { get; set; }

        public int ChecklistItemId { get; set; }
        public ChecklistItem ChecklistItem { get; set; }

        public int Score { get; set; }
        public string Comment { get; set; }

        public List<string> ImageUrls { get; set; }
    }
}
