using ProniaModular.Modules.Products.Common.Queries;

namespace ProniaModular.Modules.Products.Features.Sizes
{
    // GET /api/sizes?search=xl&sortBy=name&desc=false&page=1&pageSize=10&includeDeleted=false
    public sealed record SizeQueryParameters : QueryParametersBase
    {
        // Only sizes that are currently assigned to at least one product.
        public bool? OnlyAssigned { get; set; }
    }
}
