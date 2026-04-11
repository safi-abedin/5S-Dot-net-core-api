using api.Enums;

namespace api.DTOS.RedTags
{
    public class RedTagResponseDto
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Location { get; set; } = string.Empty;
        public List<string> PhotoUrl { get; set; } = [];
        public string ResponsiblePerson { get; set; } = string.Empty;
        public RedTagStatus Status { get; set; }
        public DateTime IdentifiedDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
