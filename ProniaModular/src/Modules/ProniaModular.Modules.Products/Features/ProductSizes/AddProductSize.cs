using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Cqrs;
using ProniaModular.Modules.Products.Common.Results;
using ProniaModular.Modules.Products.Data;
using ProniaModular.Modules.Products.Entities;

namespace ProniaModular.Modules.Products.Features.ProductSizes
{
    public sealed record AddProductSizeCommand(long ProductId, long SizeId) : ICommand<Result<ProductSizeDto>>;

    public sealed class AddProductSizeCommandValidator : AbstractValidator<AddProductSizeCommand>
    {
        public AddProductSizeCommandValidator()
        {
            RuleFor(c => c.ProductId)
                .GreaterThan(0).WithMessage("A valid product id is required.");

            RuleFor(c => c.SizeId)
                .GreaterThan(0).WithMessage("A valid size id is required.");
        }
    }

    public sealed class AddProductSizeCommandHandler(ProductsDbContext dbContext)
        : ICommandHandler<AddProductSizeCommand, Result<ProductSizeDto>>
    {
        public async Task<Result<ProductSizeDto>> Handle(AddProductSizeCommand command, CancellationToken cancellationToken = default)
        {
            var productExists = await dbContext.Products.AnyAsync(p => p.Id == command.ProductId, cancellationToken);
            if (!productExists)
                return Result<ProductSizeDto>.Failure(Error.NotFound($"Product with id {command.ProductId} was not found."));

            var size = await dbContext.Sizes.FirstOrDefaultAsync(s => s.Id == command.SizeId, cancellationToken);
            if (size is null)
                return Result<ProductSizeDto>.Failure(Error.NotFound($"Size with id {command.SizeId} was not found."));

            var alreadyAssigned = await dbContext.ProductSizes
                .AnyAsync(ps => ps.ProductId == command.ProductId && ps.SizeId == command.SizeId, cancellationToken);
            if (alreadyAssigned)
                return Result<ProductSizeDto>.Failure(Error.Conflict("This size is already assigned to the product."));

            var productSize = new ProductSize
            {
                ProductId = command.ProductId,
                SizeId = command.SizeId
            };

            dbContext.ProductSizes.Add(productSize);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<ProductSizeDto>.Success(new ProductSizeDto(command.ProductId, command.SizeId, size.Name));
        }
    }
}
