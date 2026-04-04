namespace api.Helpers.Pagination
{
    public class PagedResponse<T>
    {
        public List<T> Data { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }

        public bool HasNext => (Page * Size) < TotalCount;
    }
}
