namespace ProniaModular.Modules.Products.Common.Queries
{
    // Shared search / sort / paging parameters.
    // Bound to the query string through [AsParameters], so endpoint signatures stay short.
    public abstract record QueryParametersBase
    {
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;

        private int _page = 1;
        private int _pageSize = DefaultPageSize;

        // Free text search term.
        public string? Search { get; set; }

        // Column to sort by, e.g. "name", "price", "createdat", "id".
        public string? SortBy { get; set; }

        // true => descending order.
        public bool Desc { get; set; }

        // true => IgnoreQueryFilters(), soft deleted rows are returned as well.
        public bool IncludeDeleted { get; set; }

        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1
                ? DefaultPageSize
                : (value > MaxPageSize ? MaxPageSize : value);
        }
    }
}
