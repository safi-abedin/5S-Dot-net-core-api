using api.Enums;

namespace api.Helpers.Pagination
{
    public class AuditPaginationRequest : BasePaginationRequest
    {
        public int? ZoneId { get; set; }
        public string? AuditorName { get; set; }
        public AuditStatus? Status { get; set; }
        public decimal? MinScore { get; set; }
        public decimal? MaxScore { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public DateTime? AuditDateFrom { get; set; }
        public DateTime? AuditDateTo { get; set; }
    }
}
