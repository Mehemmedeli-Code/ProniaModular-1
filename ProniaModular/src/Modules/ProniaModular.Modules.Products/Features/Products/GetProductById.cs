using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Cqrs;
using ProniaModular.Modules.Products.Common.Results;
using ProniaModular.Modules.Products.Data;

namespace ProniaModular.Modules.Products.Features.Products
{
    public sealed record GetProductByIdQuery(long Id) : IQuery<Result<ProductDto>>;

    public sealed class GetProductByIdQueryHandler(ProductsDbContext dbContext)
        : IQueryHandler<GetProductByIdQuery, Result<ProductDto>>
    {
        public async Task<Result<ProductDto>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken = default)
        {
            var product = await dbContext.Products
                .Where(p => p.Id == query.Id)
                .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Description, p.CategoryId, p.Category.Name))
                .FirstOrDefaultAsync(cancellationToken);

            return product is null
                ? Result<ProductDto>.Failure(Error.NotFound($"Product with id {query.Id} was not found."))
                : Result<ProductDto>.Success(product);
        }
    }
}
