using ProniaModular.Modules.Products.Common.Queries;

namespace ProniaModular.Modules.Products.Features.Categories
{
    // GET /api/categories?search=men&sortBy=productcount&desc=true&page=1&pageSize=10&includeDeleted=false
    public sealed record CategoryQueryParameters : QueryParametersBase
    {
        // Only categories that already have at least this many products.
        public int? MinProductCount { get; set; }
    }
}
