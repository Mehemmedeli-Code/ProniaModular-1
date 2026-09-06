using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Cqrs;
using ProniaModular.Modules.Products.Common.Results;
using ProniaModular.Modules.Products.Data;

namespace ProniaModular.Modules.Products.Features.Products
{
    public sealed record UpdateProductCommand(long Id, string Name, decimal Price, string? Description, long CategoryId)
        : ICommand<Result<ProductDto>>;

    public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(p => p.Id)
                .GreaterThan(0).WithMessage("A valid product id is required.");

            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

            RuleFor(p => p.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.")
                .PrecisionScale(6, 2, false).WithMessage("Price must have at most 6 digits in total and 2 decimal places.");

            RuleFor(p => p.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

            RuleFor(p => p.CategoryId)
                .GreaterThan(0).WithMessage("Product must belong to a valid category.");
        }
    }

    public sealed class UpdateProductCommandHandler(ProductsDbContext dbContext)
        : ICommandHandler<UpdateProductCommand, Result<ProductDto>>
    {
        public async Task<Result<ProductDto>> Handle(UpdateProductCommand command, CancellationToken cancellationToken = default)
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);
            if (product is null)
                return Result<ProductDto>.Failure(Error.NotFound($"Product with id {command.Id} was not found."));

            var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == command.CategoryId, cancellationToken);
            if (category is null)
                return Result<ProductDto>.Failure(Error.NotFound($"Category with id {command.CategoryId} was not found."));

            var nameTaken = await dbContext.Products
                .AnyAsync(p => p.Name == command.Name && p.Id != command.Id, cancellationToken);
            if (nameTaken)
                return Result<ProductDto>.Failure(Error.Conflict($"A product named '{command.Name}' already exists."));

            product.Name = command.Name;
            product.Price = command.Price;
            product.Description = command.Description;
            product.CategoryId = command.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<ProductDto>.Success(new ProductDto(product.Id, product.Name, product.Price, product.Description, product.CategoryId, category.Name));
        }
    }
}
