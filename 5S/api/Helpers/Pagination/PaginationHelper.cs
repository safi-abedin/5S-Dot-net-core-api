using Microsoft.EntityFrameworkCore;

namespace api.Helpers.Pagination
{
    public static class PaginationHelper
    {
        public static async Task<PagedResponse<T>> CreateAsync<T>(
            IQueryable<T> query, int page, int size)
        {
            var total = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return new PagedResponse<T>
            {
                Data = data,
                TotalCount = total,
                Page = page,
                Size = size
            };
        }
    }
}
