using Microsoft.EntityFrameworkCore;

namespace ProniaModular.Modules.Products.Common.Queries
{
    public static class QueryableExtensions
    {
        // Turns the global query filter off only when the caller explicitly asks for deleted rows.
        public static IQueryable<T> ApplyDeletedFilter<T>(this IQueryable<T> query, bool includeDeleted)
            where T : class
            => includeDeleted ? query.IgnoreQueryFilters() : query;

        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (page < 1)
                page = 1;

            if (pageSize < 1)
                pageSize = QueryParametersBase.DefaultPageSize;

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<T>(items, page, pageSize, totalCount);
        }

        // Escapes LIKE wildcards typed by the user, then wraps the term with %...%.
        public static string ToLikePattern(this string term)
        {
            var escaped = term.Trim()
                .Replace("[", "[[]")
                .Replace("%", "[%]")
                .Replace("_", "[_]");

            return $"%{escaped}%";
        }
    }
}
