
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace api.DTOS.Audits
{
    public class FeedBackItemDto
    {
        public int? Id { get; set; }
        public string? Comment { get; set; }
        public List<string>? ImageUrls { get; set; } = new();

        [JsonIgnore]
        public List<IFormFile>? Images { get; set; }

        public bool? Good { get; set; } = true;
        public bool? Bad { get; set; } = false;
    }
}
