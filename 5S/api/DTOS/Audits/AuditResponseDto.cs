using api.Enums;

namespace api.DTOS.Audits
{
    public class AuditResponseDto
    {
        public int Id { get; set; }
        public int ZoneId { get; set; }

        public string ZoneName { get; set; }

        public string AuditorName { get; set; }
        public string AuditeeName { get; set; }
        public string Department { get; set; }
        public DateTime AuditDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalScore { get; set; }
        public decimal Percentage { get; set; }
        public AuditStatus Status { get; set; }
        public List<AuditItemDto> Items { get; set; } = new();
        public List<FeedBackItemDto>? FeedBackItems { get; set; }
    }
}
