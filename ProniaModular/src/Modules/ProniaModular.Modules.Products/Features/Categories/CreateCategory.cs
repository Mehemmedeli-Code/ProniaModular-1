using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Cqrs;
using ProniaModular.Modules.Products.Common.Results;
using ProniaModular.Modules.Products.Data;
using ProniaModular.Modules.Products.Entities;

namespace ProniaModular.Modules.Products.Features.Categories
{
    public sealed record CreateCategoryCommand(string Name) : ICommand<Result<CategoryDto>>;

    public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(200).WithMessage("Category name must not exceed 200 characters.");
        }
    }

    public sealed class CreateCategoryCommandHandler(ProductsDbContext dbContext)
        : ICommandHandler<CreateCategoryCommand, Result<CategoryDto>>
    {
        public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken = default)
        {
            var nameExists = await dbContext.Categories
                .AnyAsync(c => c.Name == command.Name, cancellationToken);

            if (nameExists)
                return Result<CategoryDto>.Failure(Error.Conflict($"A category named '{command.Name}' already exists."));

            var category = new Category
            {
                Name = command.Name,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<CategoryDto>.Success(new CategoryDto(category.Id, category.Name));
        }
    }
}
