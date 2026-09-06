using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Cqrs;
using ProniaModular.Modules.Products.Common.Results;
using ProniaModular.Modules.Products.Data;

namespace ProniaModular.Modules.Products.Features.Sizes
{
    public sealed record GetSizesQuery : IQuery<Result<List<SizeDto>>>;

    public sealed class GetSizesQueryHandler(ProductsDbContext dbContext)
        : IQueryHandler<GetSizesQuery, Result<List<SizeDto>>>
    {
        public async Task<Result<List<SizeDto>>> Handle(GetSizesQuery query, CancellationToken cancellationToken = default)
        {
            var sizes = await dbContext.Sizes
                .OrderBy(s => s.Name)
                .Select(s => new SizeDto(s.Id, s.Name))
                .ToListAsync(cancellationToken);

            return Result<List<SizeDto>>.Success(sizes);
        }
    }
}
