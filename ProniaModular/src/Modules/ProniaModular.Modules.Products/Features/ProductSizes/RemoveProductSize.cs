using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Cqrs;
using ProniaModular.Modules.Products.Common.Results;
using ProniaModular.Modules.Products.Data;

namespace ProniaModular.Modules.Products.Features.ProductSizes
{
    public sealed record RemoveProductSizeCommand(long ProductId, long SizeId) : ICommand<Result<bool>>;

    public sealed class RemoveProductSizeCommandValidator : AbstractValidator<RemoveProductSizeCommand>
    {
        public RemoveProductSizeCommandValidator()
        {
            RuleFor(c => c.ProductId)
                .GreaterThan(0).WithMessage("A valid product id is required.");

            RuleFor(c => c.SizeId)
                .GreaterThan(0).WithMessage("A valid size id is required.");
        }
    }

    public sealed class RemoveProductSizeCommandHandler(ProductsDbContext dbContext)
        : ICommandHandler<RemoveProductSizeCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(RemoveProductSizeCommand command, CancellationToken cancellationToken = default)
        {
            var productSize = await dbContext.ProductSizes
                .FirstOrDefaultAsync(ps => ps.ProductId == command.ProductId && ps.SizeId == command.SizeId, cancellationToken);

            if (productSize is null)
                return Result<bool>.Failure(Error.NotFound("This size is not assigned to the product."));

            dbContext.ProductSizes.Remove(productSize);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
