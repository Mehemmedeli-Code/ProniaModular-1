using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Cqrs;
using ProniaModular.Modules.Products.Common.Results;
using ProniaModular.Modules.Products.Data;

namespace ProniaModular.Modules.Products.Features.Categories
{
    public sealed record GetCategoryByIdQuery(long Id) : IQuery<Result<CategoryDto>>;

    public sealed class GetCategoryByIdQueryHandler(ProductsDbContext dbContext)
        : IQueryHandler<GetCategoryByIdQuery, Result<CategoryDto>>
    {
        public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken = default)
        {
            var category = await dbContext.Categories
                .Where(c => c.Id == query.Id)
                .Select(c => new CategoryDto(c.Id, c.Name))
                .FirstOrDefaultAsync(cancellationToken);

            return category is null
                ? Result<CategoryDto>.Failure(Error.NotFound($"Category with id {query.Id} was not found."))
                : Result<CategoryDto>.Success(category);
        }
    }
}
