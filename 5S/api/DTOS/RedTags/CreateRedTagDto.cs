using Microsoft.AspNetCore.Http;

namespace api.DTOS.RedTags
{
    public class CreateRedTagDto
    {
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Location { get; set; } = string.Empty;
        public List<IFormFile>? Photos { get; set; }
        public string ResponsiblePerson { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? IdentifiedDate { get; set; }
        public DateTime? ClosingDate { get; set; }
    }
}
