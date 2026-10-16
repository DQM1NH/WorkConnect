using Microsoft.EntityFrameworkCore;

namespace WorkConnect.Models
{
    public abstract class PagedResultBase
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public int TotalItems { get; init; }

        public string? Search { get; init; }

        public string Tab { get; init; } = string.Empty;

        public string QueryKey { get; init; } = string.Empty;
        public string PageKey { get; init; } = string.Empty;

        public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));
        public bool HasSearch => !string.IsNullOrWhiteSpace(Search);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
        public int FirstItem => TotalItems == 0 ? 0 : (Page - 1) * PageSize + 1;
        public int LastItem => Math.Min(Page * PageSize, TotalItems);
    }

    public class PagedResult<T> : PagedResultBase
    {
        public List<T> Items { get; init; } = new();

        public static async Task<PagedResult<T>> CreateAsync<TSource>(
            IQueryable<TSource> source, Func<TSource, T> map,
            int page, int pageSize, string? search,
            string tab, string queryKey, string pageKey)
        {
            var total = await source.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            var raw = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<T>
            {
                Items = raw.Select(map).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Search = search,
                Tab = tab,
                QueryKey = queryKey,
                PageKey = pageKey
            };
        }
    }
}
