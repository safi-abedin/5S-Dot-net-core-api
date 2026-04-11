using api.Enums;

namespace api.Helpers.Pagination
{
    public class RedTagPaginationRequest : BasePaginationRequest
    {
        public string? ResponsiblePerson { get; set; }
        public RedTagStatus? Status { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public DateTime? IdentifiedDateFrom { get; set; }
        public DateTime? IdentifiedDateTo { get; set; }
        public DateTime? ClosingDateFrom { get; set; }
        public DateTime? ClosingDateTo { get; set; }
    }
}
