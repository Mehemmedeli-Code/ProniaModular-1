using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Cqrs;
using ProniaModular.Modules.Products.Common.Results;
using ProniaModular.Modules.Products.Data;

namespace ProniaModular.Modules.Products.Features.Products
{
    public sealed record GetProductsQuery : IQuery<Result<List<ProductDto>>>;

    public sealed class GetProductsQueryHandler(ProductsDbContext dbContext)
        : IQueryHandler<GetProductsQuery, Result<List<ProductDto>>>
    {
        public async Task<Result<List<ProductDto>>> Handle(GetProductsQuery query, CancellationToken cancellationToken = default)
        {
            var products = await dbContext.Products
                .OrderBy(p => p.Name)
                .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Description, p.CategoryId, p.Category.Name))
                .ToListAsync(cancellationToken);

            return Result<List<ProductDto>>.Success(products);
        }
    }
}
