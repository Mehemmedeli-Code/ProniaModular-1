using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Cqrs;
using ProniaModular.Modules.Products.Common.Results;
using ProniaModular.Modules.Products.Data;
using ProniaModular.Modules.Products.Entities;

namespace ProniaModular.Modules.Products.Features.Products
{
    public sealed record CreateProductCommand(string Name, decimal Price, string? Description, long CategoryId)
        : ICommand<Result<ProductDto>>;

    public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
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

    public sealed class CreateProductCommandHandler(ProductsDbContext dbContext)
        : ICommandHandler<CreateProductCommand, Result<ProductDto>>
    {
        public async Task<Result<ProductDto>> Handle(CreateProductCommand command, CancellationToken cancellationToken = default)
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == command.CategoryId, cancellationToken);
            if (category is null)
                return Result<ProductDto>.Failure(Error.NotFound($"Category with id {command.CategoryId} was not found."));

            var nameExists = await dbContext.Products.AnyAsync(p => p.Name == command.Name, cancellationToken);
            if (nameExists)
                return Result<ProductDto>.Failure(Error.Conflict($"A product named '{command.Name}' already exists."));

            var product = new Product
            {
                Name = command.Name,
                Price = command.Price,
                Description = command.Description,
                CategoryId = command.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<ProductDto>.Success(new ProductDto(product.Id, product.Name, product.Price, product.Description, product.CategoryId, category.Name));
        }
    }
}
