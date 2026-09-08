using ProniaModular.Modules.Products.Common.Queries;

namespace ProniaModular.Modules.Products.Features.Products
{
    // GET /api/products?search=shirt&categoryId=2&sizeId=1&minPrice=10&maxPrice=99
    //                  &sortBy=price&desc=true&page=1&pageSize=10&includeDeleted=false
    public sealed record ProductQueryParameters : QueryParametersBase
    {
        public long? CategoryId { get; set; }

        public long? SizeId { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }
    }
}
