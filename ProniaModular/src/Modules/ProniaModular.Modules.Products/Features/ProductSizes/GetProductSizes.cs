using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Cqrs;
using ProniaModular.Modules.Products.Common.Results;
using ProniaModular.Modules.Products.Data;

namespace ProniaModular.Modules.Products.Features.ProductSizes
{
    public sealed record GetProductSizesQuery(long ProductId) : IQuery<Result<List<ProductSizeDto>>>;

    public sealed class GetProductSizesQueryValidator : AbstractValidator<GetProductSizesQuery>
    {
        public GetProductSizesQueryValidator()
        {
            RuleFor(q => q.ProductId)
                .GreaterThan(0).WithMessage("A valid product id is required.");
        }
    }

    public sealed class GetProductSizesQueryHandler(ProductsDbContext dbContext)
        : IQueryHandler<GetProductSizesQuery, Result<List<ProductSizeDto>>>
    {
        public async Task<Result<List<ProductSizeDto>>> Handle(GetProductSizesQuery query, CancellationToken cancellationToken = default)
        {
            var productExists = await dbContext.Products.AnyAsync(p => p.Id == query.ProductId, cancellationToken);
            if (!productExists)
                return Result<List<ProductSizeDto>>.Failure(Error.NotFound($"Product with id {query.ProductId} was not found."));

            var sizes = await dbContext.ProductSizes
                .Where(ps => ps.ProductId == query.ProductId)
                .Select(ps => new ProductSizeDto(ps.ProductId, ps.SizeId, ps.Size.Name))
                .ToListAsync(cancellationToken);

            return Result<List<ProductSizeDto>>.Success(sizes);
        }
    }
}
