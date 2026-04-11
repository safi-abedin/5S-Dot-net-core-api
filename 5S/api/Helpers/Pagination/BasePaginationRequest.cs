namespace api.Helpers.Pagination
{
    public class BasePaginationRequest
    {
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
    }
}
