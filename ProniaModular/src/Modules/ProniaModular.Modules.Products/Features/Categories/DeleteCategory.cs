using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProniaModular.Modules.Products.Common.Cqrs;
using ProniaModular.Modules.Products.Common.Results;
using ProniaModular.Modules.Products.Data;

namespace ProniaModular.Modules.Products.Features.Categories
{
    public sealed record DeleteCategoryCommand(long Id) : ICommand<Result<bool>>;

    public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
    {
        public DeleteCategoryCommandValidator()
        {
            RuleFor(c => c.Id)
                .GreaterThan(0).WithMessage("A valid category id is required.");
        }
    }

    public sealed class DeleteCategoryCommandHandler(ProductsDbContext dbContext)
        : ICommandHandler<DeleteCategoryCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken = default)
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
            if (category is null)
                return Result<bool>.Failure(Error.NotFound($"Category with id {command.Id} was not found."));

            var hasProducts = await dbContext.Products.AnyAsync(p => p.CategoryId == command.Id, cancellationToken);
            if (hasProducts)
                return Result<bool>.Failure(Error.Conflict("Category cannot be deleted while it still has products assigned to it."));

            dbContext.Categories.Remove(category);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
