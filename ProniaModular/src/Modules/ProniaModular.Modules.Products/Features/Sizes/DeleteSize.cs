using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Cqrs;
using ProniaModular.Modules.Products.Common.Results;
using ProniaModular.Modules.Products.Data;

namespace ProniaModular.Modules.Products.Features.Sizes
{
    public sealed record DeleteSizeCommand(long Id) : ICommand<Result<bool>>;

    public sealed class DeleteSizeCommandValidator : AbstractValidator<DeleteSizeCommand>
    {
        public DeleteSizeCommandValidator()
        {
            RuleFor(s => s.Id)
                .GreaterThan(0).WithMessage("A valid size id is required.");
        }
    }

    public sealed class DeleteSizeCommandHandler(ProductsDbContext dbContext)
        : ICommandHandler<DeleteSizeCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteSizeCommand command, CancellationToken cancellationToken = default)
        {
            var size = await dbContext.Sizes.FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
            if (size is null)
                return Result<bool>.Failure(Error.NotFound($"Size with id {command.Id} was not found."));

            var isAssigned = await dbContext.ProductSizes.AnyAsync(ps => ps.SizeId == command.Id, cancellationToken);
            if (isAssigned)
                return Result<bool>.Failure(Error.Conflict("Size cannot be deleted while it is still assigned to products."));

            dbContext.Sizes.Remove(size);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
