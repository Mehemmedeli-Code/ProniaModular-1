using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Cqrs;
using ProniaModular.Modules.Products.Common.Results;
using ProniaModular.Modules.Products.Data;

namespace ProniaModular.Modules.Products.Features.Categories
{
    public sealed record UpdateCategoryCommand(long Id, string Name) : ICommand<Result<CategoryDto>>;

    public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(c => c.Id)
                .GreaterThan(0).WithMessage("A valid category id is required.");

            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(200).WithMessage("Category name must not exceed 200 characters.");
        }
    }

    public sealed class UpdateCategoryCommandHandler(ProductsDbContext dbContext)
        : ICommandHandler<UpdateCategoryCommand, Result<CategoryDto>>
    {
        public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
            if (category is null)
                return Result<CategoryDto>.Failure(Error.NotFound($"Category with id {command.Id} was not found."));

            var nameTaken = await dbContext.Categories
                .AnyAsync(c => c.Name == command.Name && c.Id != command.Id, cancellationToken);
            if (nameTaken)
                return Result<CategoryDto>.Failure(Error.Conflict($"A category named '{command.Name}' already exists."));

            category.Name = command.Name;
            category.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<CategoryDto>.Success(new CategoryDto(category.Id, category.Name));
        }
    }
}
