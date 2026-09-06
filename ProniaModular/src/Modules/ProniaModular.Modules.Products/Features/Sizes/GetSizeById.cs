using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Cqrs;
using ProniaModular.Modules.Products.Common.Results;
using ProniaModular.Modules.Products.Data;

namespace ProniaModular.Modules.Products.Features.Sizes
{
    public sealed record GetSizeByIdQuery(long Id) : IQuery<Result<SizeDto>>;

    public sealed class GetSizeByIdQueryHandler(ProductsDbContext dbContext)
        : IQueryHandler<GetSizeByIdQuery, Result<SizeDto>>
    {
        public async Task<Result<SizeDto>> Handle(GetSizeByIdQuery query, CancellationToken cancellationToken = default)
        {
            var size = await dbContext.Sizes
                .Where(s => s.Id == query.Id)
                .Select(s => new SizeDto(s.Id, s.Name))
                .FirstOrDefaultAsync(cancellationToken);

            return size is null
                ? Result<SizeDto>.Failure(Error.NotFound($"Size with id {query.Id} was not found."))
                : Result<SizeDto>.Success(size);
        }
    }
}
