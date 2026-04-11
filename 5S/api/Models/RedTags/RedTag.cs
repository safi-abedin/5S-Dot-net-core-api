using api.Enums;
using api.Models.Base;

namespace api.Models.RedTags
{
    public class RedTag : BaseEntity
    {
        public string ItemName { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }

        public string Location { get; set; }
        public List<string> PhotoUrl { get; set; }

        public string ResponsiblePerson { get; set; }

        public RedTagStatus Status { get; set; }

        public DateTime IdentifiedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ClosingDate { get; set; }
    }
}
